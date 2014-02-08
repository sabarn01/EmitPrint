//#define Gen
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Collections.Concurrent;
using System.IO; 

namespace EmitPrintLib
{
    using oc = OpCodes; 
    public class BaseEmitPrinter
    {        
        delegate string ObjPrinter(object o,StringBuilder Sb = null);
        
        private static ConcurrentDictionary<Type, ObjPrinter> GeneratedTypes = new ConcurrentDictionary<Type, ObjPrinter>();

        public string EmitPrintMessage(object obj)
        {
            var Typ = obj.GetType();
            if(!GeneratedTypes.ContainsKey(Typ))
            {
                GeneratedTypes.AddOrUpdate(Typ,GenPrinter(Typ),(x,y)=>y);
            }
            ObjPrinter Printer; 
            GeneratedTypes.TryGetValue(Typ,out Printer);
            if(Printer != null)
            {
                return Printer(obj);
            }
            else
            {
                return obj.ToString(); 
            }

        }

        private string PrintObj(object Obj)
        {
            var Typ = Obj.GetType();
            if (!GeneratedTypes.ContainsKey(Typ))
            {
                GenPrinter(Typ);
            }
            return GeneratedTypes[Typ].Invoke(Obj);
        }

        private static void EmitDebugWriteLine(ILGenerator Gen,string s)
        {
            var Fn = typeof(Debug).GetMethod("WriteLine", new Type[]{ typeof(string) } );
            Debug.Assert(Fn != null);
            Gen.Emit(OpCodes.Ldstr, s);
            Gen.Emit(oc.Call, Fn);
        }

        

        private MethodInfo LookupMI(Type T,String Name)
        {
            return LookupMI(T, Name, Type.EmptyTypes);
        }

        private MethodInfo LookupMI(Type T,String Name, Type[] Args )
        {
            var MI = T.GetMethod(Name, Args);
            Debug.Assert(MI != null);
            return MI; 
        }

        enum Methods
        {
            SBAppendS = 0,
            SBAppendS_O = 1,
            OToString = 2,
        }

        

        MethodInfo[] _MethodMap = null;
        MethodInfo[] MethodMap
        {
            get
            {
                if(_MethodMap == null )
                {
                    var len = Enum.GetValues(typeof(Methods)).Length;
                    _MethodMap = new MethodInfo[len]; 
                    var SBType = typeof(StringBuilder);
                    //StringBuilder.Append(string)
                    _MethodMap[(int)Methods.SBAppendS] = LookupMI(SBType, "Append", new Type[] { typeof(string) });
                    //stringBuilder.Append(string,object)                    
                    _MethodMap[(int)Methods.SBAppendS_O] = LookupMI(SBType, "AppendFormat", new Type[] { typeof(string), typeof(object) });
                    //Object.ToString()
                    _MethodMap[(int)(Methods.OToString)] = LookupMI(typeof(object),"ToString");
                }
                return _MethodMap; 
            }
        }
        

     
        private ObjPrinter GenPrinter(Type Typ)
        {
            const string PrintObj = "PrintObj";
            

#if Gen
            var AsmName = new AssemblyName("Foo");
            var AsmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AsmName, AssemblyBuilderAccess.RunAndSave);
            
            var Mod = AsmBuilder.DefineDynamicModule("Foo","foo.dll");            
            var TB = Mod.DefineType("Foo",TypeAttributes.Class |TypeAttributes.Public | TypeAttributes.BeforeFieldInit);                        
            var DYMethod = TB.DefineMethod(PrintObj,MethodAttributes.Public|MethodAttributes.Static,typeof(string),new Type[]{typeof(object),typeof(StringBuilder)});
#else
            var DYMethod = new DynamicMethod(PrintObj, typeof(string), new Type[] { typeof(object),typeof(StringBuilder) },true  );
#endif
            
            var Gen = DYMethod.GetILGenerator();       

            HandelPossibleNullPassedInStringBuilder(Gen);
            //Local SB should be the passed in SB or a new one 
            //**********************************************
            GenerateObjectHeader(Typ, Gen);                                    
            GeneratePropertyPrinter(Typ, Gen );
            GenerateFooterPrinter(Gen);


#if Gen
            Mod.CreateGlobalFunctions();
            TB.CreateType(); 
            //DYMethod.CreateDelegate(typeof(ObjPrinter));
            
            AsmBuilder.Save(@"bob.dll");
#endif
            var x = DYMethod.CreateDelegate(typeof(ObjPrinter));
            return (ObjPrinter)x;
        }

        private void GenerateFooterPrinter(ILGenerator Gen)
        {
            Gen.Emit(OpCodes.Ldstr, "}");
            Gen.Emit(OpCodes.Call, MethodMap[(int)Methods.SBAppendS] );
            Gen.Emit(OpCodes.Callvirt, MethodMap[(int)Methods.OToString]);
            Gen.Emit(OpCodes.Ret);
        }

        private  void GeneratePropertyPrinter(Type Typ, ILGenerator Gen)
        {
            bool FirstPass = true;
            foreach (var prop in Typ.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var PropTypeTS = prop.PropertyType.GetMethod("ToString",BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod |BindingFlags.DeclaredOnly,null, new Type[] { },null);

                if ( PropTypeTS != null)
                {
                    var GetMethod = prop.GetGetMethod(false);
                    if (GetMethod != null)
                    {
                        string FmtStr = " = {0}";
                        if (FirstPass)
                        {
                            Gen.Emit(OpCodes.Ldstr, prop.Name + FmtStr);
                        }
                        else
                        {
                            Gen.Emit(OpCodes.Ldstr, "," + prop.Name + FmtStr);
                        }
                        FirstPass = false;

                        GenPropertyGetter(Gen, prop, GetMethod);
                        Gen.Emit(OpCodes.Call,MethodMap[(int)Methods.SBAppendS_O] );
                    }
                }
            }
        }

        protected LocalBuilder TheStringBuilder { get; set; }

        private void GenerateObjectHeader(Type Typ, ILGenerator Gen)
        {
            Gen.Emit(OpCodes.Ldloc, TheStringBuilder);
            Gen.Emit(OpCodes.Ldstr, Typ.Name + "{");
            Gen.Emit(OpCodes.Call, MethodMap[(int)Methods.SBAppendS] );
        }

        protected virtual void HandelPossibleNullPassedInStringBuilder(ILGenerator Gen)
        {
            var SBType = typeof (StringBuilder); 
            var SB = Gen.DeclareLocal(SBType);
            TheStringBuilder = SB; 
            var SBIsNotNull = Gen.DefineLabel();
            var JumpOverArg1LclAssign = Gen.DefineLabel();
            EmitDebugWriteLine(Gen, "IN Dy Methods");
            //Push Arg 1 on stack 
            Debug.Print("Before LDArg_1 offset = " + Gen.ILOffset.ToString());
            Gen.Emit(OpCodes.Ldarg_1);
            Debug.Print("After LDArg_1 offset = " + Gen.ILOffset.ToString());
            //If Arg1 is not null jump to Assigment
            Gen.Emit(OpCodes.Brtrue, SBIsNotNull);
            //**********************************************************
            //Arg1 is null 
            //Stack Empty 
            //Call constructor             
            Debug.Print(Gen.ILOffset.ToString());
            Debug.Print("SBIsNull offset = " + Gen.ILOffset.ToString());
            Gen.Emit(OpCodes.Newobj, SBType.GetConstructor(new Type[] { }));
            Gen.Emit(OpCodes.Castclass, SBType);
            Gen.Emit(OpCodes.Stloc, SB);
            //Stack Should be empty 
            //*************************************
            //JUmp over assgiment from arg1 

            Gen.Emit(OpCodes.Br, JumpOverArg1LclAssign);
            Gen.MarkLabel(SBIsNotNull);
            Debug.Print("SBIsNotNull offset = " + Gen.ILOffset.ToString());
            EmitDebugWriteLine(Gen, "Arg1 is not null");
            Gen.Emit(OpCodes.Ldarg_1);
            Gen.Emit(OpCodes.Stloc, SB);
            //Stack Should be empty 
            Gen.MarkLabel(JumpOverArg1LclAssign);
            
        }

        private static void GenPropertyGetter(ILGenerator Gen, PropertyInfo prop, MethodInfo GetMethod)
        {
            Gen.Emit(OpCodes.Ldarg_0);
            Gen.Emit(OpCodes.Castclass, prop.DeclaringType);            
            Gen.Emit(OpCodes.Callvirt, GetMethod);
            if (prop.PropertyType.IsValueType)
            {
                Gen.Emit(OpCodes.Box, prop.PropertyType);
            }
        }
    }
}
