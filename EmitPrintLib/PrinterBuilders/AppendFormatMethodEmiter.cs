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
        
        #region Properties
              
        #endregion
        #region Constructors

        internal AppendFormatMethodEmiter(Type ObjectType,TypeBuilder TB)
        {

            _AppendFormatBuilder = TB.DefineMethod("AppendFormat", MethodAttribs, CallingConventions.HasThis, typeof(void), new Type[] { ObjectType, typeof(StringBuilder),typeof(IEmitTypeCache) });
            _Gen = _AppendFormatBuilder.GetILGenerator();
            _TypeBuilder = TB; 
            _PrinterType = ObjectType; 
        }

        public static bool HasToStringOverload(object o)
        {
            return o.GetType().GetMethod("ToString", EmitUtil.Bindings.PublicInstDeclaredOnly) != null;             
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
            
 	        foreach(var prop in _PrinterType.GetProperties())            
            {
                //Set up the first iteration by putting the string builder on the stack
                _Gen.Emit(OC.Ldarg_2);
                if(HasToStringOverload(prop.PropertyType))
                {
                    string appenFmtStr = prop.Name + " = {0} ; ";
                    string DebugMsg = "Calling Append for property " + prop.Name;                    
                    PrintDebugInfo(DebugMsg);             
                    _Gen.Emit(OC.Ldstr,appenFmtStr);
                    _Gen.Emit(OC.Ldarg_1);
                    _Gen.Emit(OC.Callvirt,prop.GetGetMethod());
                    if(prop.PropertyType.IsValueType)
                    {
                        _Gen.Emit(OC.Box,prop.PropertyType); 
                    }
                    //_Gen.Emit(OC.Ldstr, "Test");
                    _Gen.Emit(OC.Call,KnownMethods.StringBuilderMethods.Append_Format_ObjectMI);
                    PrintDebugInfo("End " + DebugMsg); 
                    //Next iteration is good because SB append returns sb 
                }
                //We want the stack to be unaffected after this method so clear the SB 
                _Gen.Emit(OC.Pop); 
            }
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

        #endregion

    }
}
