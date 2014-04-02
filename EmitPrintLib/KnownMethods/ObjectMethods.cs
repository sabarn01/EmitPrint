namespace EmitPrintLib.KnownMethods
{
    using System;
    using System.Reflection;

    internal static class ObjectMethods
    {
        public static readonly MethodInfo GetTypeMI;

        public static readonly Type ObjType = typeof(object);

        static ObjectMethods()
        {
            ToStringMI = EmitUtil.LookupMI(typeof(object), "ToString");

            GetTypeMI = ObjType.LookupMIOrFail("GetType");
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> object which represents the base toString Method
        /// </summary>
        public static MethodInfo ToStringMI { get; private set; }
    }
}
