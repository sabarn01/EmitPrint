using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EmitPrintLib
{
    static class  EmitUtil
    {   
        
        public static MethodInfo LookupMI(this Type T, string Name)
        {
            return LookupMI(T, Name, null); 
        }
        public static MethodInfo LookupMI(this Type T, string Name, params Type[] Args)
        {
            Args = Args ?? Type.EmptyTypes; 
            var MI = T.GetMethod(Name, Bindings.PublicInst, null, Args, null);            
            return MI; 
        }

        public static MethodInfo LookupMIOrFail(this Type T, string Name)
        {
            return LookupMIOrFail(T, Name, null);
        }
        public static MethodInfo LookupMIOrFail(this Type T, string Name,  params Type[] Args)
        {
            Args = Args ?? Type.EmptyTypes;
            var MI = T.GetMethod(Name,Bindings.PublicInst, null, Args, null);            
            if(MI == null)
            {
                throw new InvalidOperationException("Can't Find method " + Name + " on type " + T.Name); 
            }
            return MI;
        }

        [Conditional("DEBUG")]
        public static void EmitDebugWriteLine(this ILGenerator Gen, string s)
        {
            var Fn = typeof(Debug).GetMethod("WriteLine", new Type[] { typeof(string) });
            Debug.Assert(Fn != null);
            Gen.Emit(OpCodes.Ldstr, s);
            Gen.Emit(OpCodes.Call, Fn);
        }
        
        [Conditional("DEBUG")]
        public static void PrintILPosition(this ILGenerator Gen,string Message)
        {
            Debug.WriteLine("{0} at offset = {1:X8} ", Message, Gen.ILOffset); 
        }

        public static class Bindings
        {
            public const BindingFlags NonPublicInst = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic;
            public const BindingFlags PublicInst = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public;
            public const BindingFlags PublicStatic = BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public;
            public const BindingFlags NonPublicStatic = BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic;
            public const BindingFlags PublicInstDeclaredOnly = PublicInst | BindingFlags.DeclaredOnly;
        }

        
    }
}
