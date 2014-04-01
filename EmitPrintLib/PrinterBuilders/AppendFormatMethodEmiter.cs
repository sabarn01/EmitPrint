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
    internal sealed class AppendFormatMethodEmiter
    {
        const MethodAttributes MethodAttribs = MethodAttributes.Virtual | MethodAttributes.Public;        
        private readonly MethodBuilder _AppendFormatBuilder = null;
        private readonly ILGenerator _Gen =null;
        private readonly TypeBuilder _TypeBuilder = null;
        private readonly Type _PrinterType;
        private readonly ModuleBuilder _Modlue;
        private readonly MethodInfo _EmitMI;
        private readonly MethodInfo _AppendFormat;
        private readonly IEmitTypeCache _Cache; 
        #region Properties
          
        #endregion
        #region Constructors

        internal AppendFormatMethodEmiter(Type ObjectType,TypeBuilder TB,ModuleBuilder Module,IEmitTypeCache Cache)
        {

            _AppendFormatBuilder = TB.DefineMethod("AppendFormat", MethodAttribs, CallingConventions.HasThis, typeof(void), new Type[] { ObjectType, typeof(StringBuilder),typeof(IEmitTypeCache) });
            _Gen = _AppendFormatBuilder.GetILGenerator();
            _TypeBuilder = TB; 
            _PrinterType = ObjectType;
            _Modlue = Module;
            var T = typeof(TypeEmiter);
            _EmitMI = typeof(TypeEmiter).GetMethod("Emit", EmitUtil.Bindings.NonPublicStatic);
            Debug.Assert(_EmitMI != null); 
            _Cache = Cache; 
        }
        #endregion
        MethodInfo GetEmitGeneric(Type T)
        {
            return _EmitMI.MakeGenericMethod(T);
        }

        public static bool HasToStringOverload(Type o)
        {
            return o.GetMethod("ToString", EmitUtil.Bindings.PublicInstDeclaredOnly,null,Type.EmptyTypes,null) != null; 
        }

        [Conditional("DEBUG")]
        private void PrintDebugInfo(String Message)
        {
            _Gen.PrintILPosition(Message);
            _Gen.EmitDebugWriteLine(Message);
        }

        internal void CreateMethod()
        {
            GenNullCheck(); 
            EnumerateProperites();
            GenReturn(); 
        }

        private void GenReturn()
        {
            _Gen.Emit(OC.Ret);
        }

        

        private void EnumerateProperites()
        {

            _Gen.Emit(OC.Ldarg_2);
 	        foreach(var prop in _PrinterType.GetProperties())            
            {
                //Set up the first iteration by putting the string builder on the stack
                
                if (HasToStringOverload(prop.PropertyType))
                {
                    GeneratePrintProperty(prop);
                    //Next iteration is good because SB append returns sb 
                }
                else
                {
                    var mi = EnsureTypeIsInCache(prop.PropertyType);
                    GenAppendCall(prop ,mi);                                        
                }
                //We want the stack to be unaffected after this method so clear the SB                  
            }
            _Gen.Emit(OC.Pop);
        }

        private void GenAppendCall(PropertyInfo Prop, MethodInfo AppendFormateMi)
        {
            _Gen.Emit(OC.Ldstr, Prop.Name + " = {");
            _Gen.Emit(OC.Call, KnownMethods.StringBuilderMethods.Append_StringMI);
            _Gen.Emit(OC.Pop);
            //Load the Type Cache onto stack 
            _Gen.Emit(OC.Ldarg_3);            
            _Gen.Emit(OC.Callvirt, KnownMethods.IEmitTypeCacheMethods.GetPrinter_ClosedT(Prop.PropertyType));
            CallPropGet(Prop);
            _Gen.Emit(OC.Ldarg_2);
            _Gen.Emit(OC.Ldarg_3);
            _Gen.Emit(OC.Callvirt, AppendFormateMi);
            _Gen.Emit(OC.Ldarg_2);
            _Gen.Emit(OC.Ldstr, "};");
            _Gen.Emit(OC.Call, KnownMethods.StringBuilderMethods.Append_StringMI); 

        }

        private MethodInfo EnsureTypeIsInCache(Type printerType)
        {
            var mi = GetType().GetMethod("GenEnsureTypeIsInCache",EmitUtil.Bindings.NonPublicInst);
            Debug.Assert(mi != null);
            mi = mi.MakeGenericMethod(printerType);
            return (MethodInfo) mi.Invoke(this, Type.EmptyTypes); 
        }

        /// <summary>
        /// This function will get a printer from the cache if it exists if it doesn't 
        /// it will create it 
        /// </summary>
        /// 
        /// <param name="_PrinterType"></param>
        /// <returns>
        /// The closed generic mehtod info 
        /// </returns>
        private MethodInfo GenEnsureTypeIsInCache<T>()
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
            return typePrinter.GetType().GetMethods().Where(x=> x.Name == "AppendFormat" ).First();
            
        }
        /// <summary>
        /// This function generates a sinle print of a proerty that has an overloaded 
        /// ToString 
        /// </summary>
        /// <Preconditon>
        /// There must be a string builder on the stack when this method is called
        /// or it will generate an ivalid image 
        /// </Preconditon>
        /// <param name="prop"></param>
        private void GeneratePrintProperty(PropertyInfo prop)
        {
            string appenFmtStr = prop.Name + " = {0} ; ";
            string DebugMsg = "Calling Append for property " + prop.Name;
            //IF there is an overload for my type on string build call that 
            var typeSpesifMI = KnownMethods.StringBuilderMethods.Append_ByType(prop.PropertyType);
            if(typeSpesifMI != null)
            {
                CallPropGet(prop);
                _Gen.Emit(OC.Call, typeSpesifMI);
            }
            else
            {
                PrintDebugInfo(DebugMsg);
                _Gen.Emit(OC.Ldstr, appenFmtStr);
                CallPropGet(prop);
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
            _Gen.Emit(OC.Brtrue,ObjNotFalselbl);
            EmitUtil.EmitDebugWriteLine(_Gen, "Argument is NULL");
            _Gen.Emit(OC.Ldarg_2);
            _Gen.Emit(OC.Ldstr, "{ NULL }");
            _Gen.Emit(OC.Callvirt,KnownMethods.StringBuilderMethods.Append_StringMI);
            _Gen.Emit(OC.Pop);
            _Gen.Emit(OC.Ret);
            _Gen.MarkLabel(ObjNotFalselbl);
            EmitUtil.EmitDebugWriteLine(_Gen, "Argument Not null");
            _Gen.PrintILPosition("End GenNUllCheck" );
        }

        private bool GenToStringOveloadGuard()
        {
            var mi = _PrinterType.GetMethod("ToString", EmitUtil.Bindings.PublicInstDeclaredOnly);
            if (mi != null)
            {
                //Use the overloaded to string
                _Gen.PrintILPosition("Begin GenToStringOveloadGuard" );
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
