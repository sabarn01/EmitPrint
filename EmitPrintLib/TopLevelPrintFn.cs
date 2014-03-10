using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions; 

namespace EmitPrintLib
{
    using OC = OpCodes; 
    /// <summary>
    /// This class generates a function that takes an object and creates a string builder then
    /// passes it to a Property Printer generator
    /// </summary>
    class TopLevelPrintFnGenerator : MethodEmiter<ObjPrinter>
    {        
        private ILGenerator Gen = null;
        private dynamic Method = null;
        private Type PassedInType = null;
        private LocalBuilder SB;
        private LocalBuilder Result; 
        private Label AfterExecption;
        private Label EndOfMethod;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Creator"></param>
        public TopLevelPrintFnGenerator(Type InputType,IMethodCreator Creator)
        {
            Guard.ArgumentNotNull(InputType, "InputType");
            Guard.ArgumentNotNull(Creator, "Creator");
            Method = Creator.CreateMethod("PrintObject", typeof(string), new Type[]{ typeof(object) });
            Gen = Method.GetILGenerator();
            PassedInType = InputType;
            CreateMethod();
            Creator.SealMethod(); 

        }
        
        private void CreateMethod()
        {
            AfterExecption = Gen.DefineLabel();
            EndOfMethod = Gen.DefineLabel(); 
            GenNullCheck();
            if (!GenToStringOveloadGuard())
            {
                GenTryBlock();
                GenCreateLocals();
                CallPropertyPrinter();                
                GenCatchAll();
                CloseTryBlock();
                ReturnValue();
            }
            
        }

        private void ReturnValue()
        {
            Gen.PrintILPosition("Begin ReturnValue");
            Gen.MarkLabel(EndOfMethod);
            Gen.Emit(OC.Ldloc, Result);
            Gen.Emit(OC.Ret);
        }

        private void CloseTryBlock()
        {
            Gen.EndExceptionBlock();
            Gen.MarkLabel(AfterExecption);
        }

        private void CallPropertyPrinter()
        {           
            
            Gen.Emit(OC.Ldstr, "HI");
            Gen.Emit(OC.Stloc, Result);
            Gen.Emit(OC.Leave, EndOfMethod); 
        }

        private void GenCatchAll()
        {
            Gen.BeginCatchBlock(typeof(Exception));
            Gen.Emit(OC.Ldstr, "{ Exception }");
        }

        private void GenTryBlock()
        {
            Gen.PrintILPosition("Begin GenTryBlock" );                
            Gen.BeginExceptionBlock();
            Gen.PrintILPosition("Begin GenTryBlock");                
        }     

        private void GenCreateLocals()
        {
            Gen.PrintILPosition("Begin GenCreateSB");                
            SB = Gen.DeclareLocal(typeof(StringBuilder));
            Result = Gen.DeclareLocal(typeof(string)); 
            Gen.PrintILPosition("Begin GenCreateSB" );                
        }

        /// <summary>
        /// This function makes sure that the type has its own to string we call it 
        /// </summary>
        private bool GenToStringOveloadGuard()
        {
            var mi = PassedInType.GetMethod("ToString", EmitUtil.Bindings.PublicInstDeclaredOnly);
            if(mi != null)
            {
                //Use the overloaded to string
                Gen.PrintILPosition("Begin GenToStringOveloadGuard");                
                Gen.Emit(OC.Ldstr, "{{ {0} }}");
                Gen.Emit(OC.Ldarg_0);
                Gen.Emit(OC.Call, mi);
                Gen.Emit(OC.Call, KnownMethods.StringBuilderMethods.Append_Format_ObjectMI);
                Gen.Emit(OC.Tailcall);
                Gen.Emit(OC.Callvirt, KnownMethods.ObjectMethods.ToStringMI);
                Gen.PrintILPosition("End GenToStringOveloadGuard");    
                return true; 
            }
            return false; 
        }

        /// <summary>
        /// Validate that the object passe into the genated function is not null if so return { NULL }
        /// </summary>
        private void GenNullCheck()
        {
            Gen.PrintILPosition("Begin GenNUllCheck");
            var ObjNotFalselbl = Gen.DefineLabel(); 
            Gen.Emit(OC.Ldarg_0);
            Gen.Emit(OC.Brtrue,ObjNotFalselbl);
            EmitUtil.EmitDebugWriteLine(Gen, "Argument is NULL");
            Gen.Emit(OC.Ldstr, "{ NULL }");
            Gen.Emit(OC.Ret);
            Gen.MarkLabel(ObjNotFalselbl);
            EmitUtil.EmitDebugWriteLine(Gen, "Argument Not null");
            Gen.PrintILPosition("End GenNUllCheck");
        }

        public ObjPrinter CreateDelegate()
        {
            return (ObjPrinter) Method.CreateDelegate(typeof(ObjPrinter)); 
        }

        public MethodBuilder GetMethod()
        {
            throw new NotImplementedException();
        }
    }
}
