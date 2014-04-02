namespace EmitPrintLib.PrinterBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text;
    using System.Threading.Tasks;
    using OC = System.Reflection.Emit.OpCodes;

    internal sealed class AppendFormatGenerator
    {
        #region Fields
        private const MethodAttributes MethodAttribs = MethodAttributes.Virtual | MethodAttributes.Public;
        private readonly MethodBuilder _AppendFormatBuilder = null;
        private readonly ILGenerator _Gen = null;
        private readonly TypeBuilder _TypeBuilder = null;
        private readonly Type _PrinterType;
        private readonly ModuleBuilder _Modlue;               
        private readonly IEmitTypeCache _Cache;
        #endregion

        #region Constructors
        internal AppendFormatGenerator(Type ObjectType, TypeBuilder TB, ModuleBuilder Module, IEmitTypeCache Cache)
        {
            _AppendFormatBuilder = TB.DefineMethod("AppendFormat", MethodAttribs, CallingConventions.HasThis, typeof(void), new Type[] { ObjectType, typeof(StringBuilder), typeof(IEmitTypeCache) });
            _Gen = _AppendFormatBuilder.GetILGenerator();
            _TypeBuilder = TB;
            _PrinterType = ObjectType;
            _Modlue = Module;            
            _Cache = Cache;
        }
        #endregion
        
        internal void CreateMethod()
        {
            GenNullCheck();
            EnumerateProperites();
            GenReturn();
        }        

        [Conditional("DEBUG")]
        private void PrintDebugInfo(string Message)
        {
            _Gen.PrintILPosition(Message);
            _Gen.EmitDebugWriteLine(Message);
        }      

        private void GenReturn()
        {
            _Gen.Emit(OC.Ret);
        }

        /// <summary>
        /// EnumerateProperties loops through all public properties 
        /// and either calls the properties overloaded to string or 
        /// gets the IPrinter from the cache for the type. 
        /// </summary>
        /// <remarks>
        /// Before the for each a <see cref="StringBuilder"/> is placed onto the 
        /// stack so any function called from inside this loop must be aware of that 
        /// state.  The also must not pop the <see cref="StringBulder"/> off of the stack
        /// </remarks>
        private void EnumerateProperites()
        {            
            bool isFirst = true;

            // Set up the first iteration by putting the string builder on the stack
            _Gen.Emit(OC.Ldarg_2);
            foreach (var prop in _PrinterType.GetProperties())
            {
                // Add a semicolon before the property unless this is the first property
                if (!isFirst)
                {
                    _Gen.Emit(OC.Ldstr, " ; ");
                    _Gen.Emit(OC.Call, KnownMethods.StringBuilderMethods.Append_StringMI);
                }
                else
                {
                    isFirst = false;
                }
                
                if (EmitUtil.HasToStringOverload(prop.PropertyType))
                {
                    GeneratePrintProperty(prop);                    
                }
                else
                {
                    var mi = EnsureTypeIsInCache(prop.PropertyType);
                    GenAppendCall(prop, mi);
                }                
            }

            // We want the stack to be unaffected after this method so clear the SB                  
            _Gen.Emit(OC.Pop);
        }

        private void GenAppendCall(PropertyInfo Prop, MethodInfo AppendFormateMi)
        {
            // Append The standard propery name = { before calling the nested type append format
            _Gen.Emit(OC.Ldstr, Prop.Name + " = {");
            _Gen.Emit(OC.Call, KnownMethods.StringBuilderMethods.Append_StringMI);

            // Append leaves the sb on the stack we need to pop it off 
            _Gen.Emit(OC.Pop);

            // Load the Type Cache onto stack 
            _Gen.Emit(OC.Ldarg_3);

            // This returns the Iprinter<T> we want we are now calling.appendformat(T,stringbuilder,TypeCache)
            _Gen.Emit(OC.Callvirt, KnownMethods.IEmitTypeCacheMethods.GetPrinter_ClosedT(Prop.PropertyType));
            CallPropGet(Prop);
            _Gen.Emit(OC.Ldarg_2);
            _Gen.Emit(OC.Ldarg_3);
            _Gen.Emit(OC.Callvirt, AppendFormateMi);

            // Load the string builder back onto the stack
            _Gen.Emit(OC.Ldarg_2);
            _Gen.Emit(OC.Ldstr, "}");
            _Gen.Emit(OC.Call, KnownMethods.StringBuilderMethods.Append_StringMI);
        }

        private MethodInfo EnsureTypeIsInCache(Type printerType)
        {
            var mi = GetType().GetMethods(  EmitUtil.MethodBindings.NonPublicInst).Where(
                x => x.Name == "EnsureTypeIsInCache" && x.IsGenericMethod 
            ).FirstOrDefault();
            Debug.Assert(mi != null, "can't find generic EnsureTypeIsInCache");
            mi = mi.MakeGenericMethod(printerType);
            return (MethodInfo)mi.Invoke(this, Type.EmptyTypes);
        }

        /// <summary>
        /// This function will get a printer from the cache if it exists if it doesn't 
        /// it will create it 
        /// </summary>        
        /// <typeparam name="T">The type of the printer desired </typeparam>
        /// <returns>
        /// The closed generic method info 
        /// </returns>
        private MethodInfo EnsureTypeIsInCache<T>()
        {
            IPrinter<T> typePrinter = null;
            if (_Cache.TypeEmited(typeof(T)))
            {
                typePrinter = _Cache.GetPrinter<T>();
            }
            else
            {
                typePrinter = TypeEmiter.Emit<T>(_Modlue, _Cache);
                _Cache.AddPrinter(typePrinter);
            }

            return typePrinter.GetType().GetMethods().Where(x => x.Name == "AppendFormat").First();
        }

        /// <summary>
        /// This function generates a single print of a property that has an overloaded 
        /// ToString 
        /// </summary>
        /// <Preconditon>
        /// There must be a string builder on the stack when this method is called
        /// or it will generate an invalid image 
        /// </Preconditon>
        /// <param name="prop"> the PropertyInfo object for the property to generate a 
        /// print call        
        /// </param>
        private void GeneratePrintProperty(PropertyInfo prop)
        {
            string appenFmtStr = prop.Name + " = ";
            string DebugMsg = "Calling Append for property " + prop.Name;            
            _Gen.Emit(OC.Ldstr, appenFmtStr);
            _Gen.Emit(OC.Call, KnownMethods.StringBuilderMethods.Append_StringMI);

            // see if a Append(T) exists for the current property type 
            var typeSpesifMI = KnownMethods.StringBuilderMethods.Append_ByType(prop.PropertyType);
            if (typeSpesifMI != null)
            {
                // It does so call the correct Apppend
                CallPropGet(prop);
                _Gen.Emit(OC.Call, typeSpesifMI);
            }
            else
            {
                // It does not call the Append(object) overload 
                PrintDebugInfo(DebugMsg);
                _Gen.Emit(OC.Ldstr, appenFmtStr);
                CallPropGet(prop);

                // Box value types 
                if (prop.PropertyType.IsValueType)
                {
                    _Gen.Emit(OC.Box, prop.PropertyType);
                }

                _Gen.Emit(OC.Call, KnownMethods.StringBuilderMethods.Append_Format_ObjectMI);
            }

            PrintDebugInfo("End " + DebugMsg);
        }

        private void CallPropGet(PropertyInfo prop)
        {
            _Gen.Emit(OC.Ldarg_1);
            _Gen.Emit(OC.Callvirt, prop.GetGetMethod());
        }

        private void GenNullCheck()
        {
            _Gen.PrintILPosition("Begin GenNUllCheck");
            var ObjNotFalselbl = _Gen.DefineLabel();
            _Gen.Emit(OC.Ldarg_1);
            _Gen.Emit(OC.Brtrue, ObjNotFalselbl);
            EmitUtil.EmitDebugWriteLine(_Gen, "Argument is NULL");
            _Gen.Emit(OC.Ldarg_2);
            _Gen.Emit(OC.Ldstr, "{ NULL }");
            _Gen.Emit(OC.Callvirt, KnownMethods.StringBuilderMethods.Append_StringMI);
            _Gen.Emit(OC.Pop);
            _Gen.Emit(OC.Ret);
            _Gen.MarkLabel(ObjNotFalselbl);
            EmitUtil.EmitDebugWriteLine(_Gen, "Argument Not null");
            _Gen.PrintILPosition("End GenNUllCheck");
        }

        private bool GenToStringOveloadGuard()
        {
            var mi = _PrinterType.GetMethod("ToString", EmitUtil.MethodBindings.PublicInstDeclaredOnly);
            if (mi != null)
            {
                // Use the overloaded to string
                _Gen.PrintILPosition("Begin GenToStringOveloadGuard");
                _Gen.Emit(OC.Ldstr, "{{ {0} }}");
                _Gen.Emit(OC.Ldarg_0);
                _Gen.Emit(OC.Call, mi);
                _Gen.Emit(OC.Call, KnownMethods.StringBuilderMethods.Append_Format_ObjectMI);
                _Gen.Emit(OC.Tailcall);
                _Gen.Emit(OC.Callvirt, KnownMethods.ObjectMethods.ToStringMI);
                _Gen.PrintILPosition("End GenToStringOveloadGuard");
                return true;
            }

            return false;
        }
    }
}
