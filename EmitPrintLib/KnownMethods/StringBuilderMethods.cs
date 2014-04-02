namespace EmitPrintLib.KnownMethods
{
    using System;
    using System.Linq;
    using System.Reflection;
    
    internal static class StringBuilderMethods
    {
        private static Type StringBuilderType = typeof(System.Text.StringBuilder);

        static StringBuilderMethods()
        {
            var sb = typeof(System.Text.StringBuilder);
            Append_StringMI = EmitUtil.LookupMI(StringBuilderType, "Append", typeof(string));
            Append_Format_ObjectMI = EmitUtil.LookupMI(StringBuilderType, "AppendFormat", typeof(string), typeof(object));
        }

        /// <summary>
        /// Gets the method info that represents the Append(object) method 
        /// </summary>
        public static MethodInfo Append_Format_ObjectMI { get; private set; }

        /// <summary>
        /// Gets the method info that represents the Append(string) method
        /// </summary>
        public static MethodInfo Append_StringMI { get; private set; }

        /// <summary>
        /// This function looks up append overloads by type 
        /// </summary>
        /// <param name="t">the type of the append overload to find </param>
        /// <remarks>This function can return null</remarks>
        /// <returns>The correct method info or null</returns>
        public static MethodInfo Append_ByType(Type t)
        {
            return StringBuilderType.LookupMI("Append", new Type[] { t });
        }
    }
}
