using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmitPrintLib.KnownMethods
{
    static class ObjectMethods
    {
        public static readonly Type ObjType = typeof(object);

        public static readonly MethodInfo GetTypeMI;
        static ObjectMethods()
        {
            ToStringMI = EmitUtil.LookupMI(typeof(Object), "ToString"); 

            GetTypeMI = ObjType.LookupMIOrFail("GetType"); 
        }
        public static MethodInfo ToStringMI { get; private set; }
    }

    public static class StringBuilderMethods
    {
        static Type sbtype = typeof(System.Text.StringBuilder);
        static StringBuilderMethods()
        {
            var sb = typeof(System.Text.StringBuilder);
            Append_StringMI = EmitUtil.LookupMI(sbtype, "Append", typeof(string));
            //Append_Format_StringMI = EmitUtil.LookupMI(sbtype, "AppendFormat", typeof(string));
            Append_Format_ObjectMI = EmitUtil.LookupMI(sbtype, "AppendFormat",typeof(string), typeof(Object)); 
        }
        public static MethodInfo Append_StringMI { get; private set; }
        //public static MethodInfo Append_Format_StringMI { get; private set; }
        public static MethodInfo Append_Format_ObjectMI { get; private set; }

    }
        
 

    public static class IEmitTypeCacheMethods
    {
        static Type CacheType = typeof(IEmitTypeCache);

        public static readonly MethodInfo TypeEmited_TypeMI;
        
        public static readonly MethodInfo GetPrinter_OpenT;

        static IEmitTypeCacheMethods()
        {
            TypeEmited_TypeMI = CacheType.LookupMIOrFail("TypeEmited",typeof(Type));

            GetPrinter_OpenT = CacheType.GetMethods().Where(
                x => x.Name == "GetPrinter" &&
                x.IsGenericMethod == true
                ).First(); 
        }

        public static MethodInfo GetPrinter_ClosedT(Type T)
        {
            return GetPrinter_OpenT.MakeGenericMethod(T);
        }

    }


}
