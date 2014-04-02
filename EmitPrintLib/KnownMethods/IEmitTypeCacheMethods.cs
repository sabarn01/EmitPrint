namespace EmitPrintLib.KnownMethods
{
    using System;
    using System.Linq;
    using System.Reflection;

    internal static class IEmitTypeCacheMethods
    {
        /// <summary>
        /// Gets the <see cref="MethodInfo"/> object that represents the function 
        /// <see cref="IEmitTypeCache.GetPrinter{T}"/>
        /// </summary>
        public static readonly MethodInfo GetPrinter_OpenT;

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> object that represents the function 
        /// <see cref="IEmitTypeCache.TypeEmited"/>
        /// </summary>
        public static readonly MethodInfo TypeEmited_TypeMI;

        private static Type CacheType = typeof(IEmitTypeCache);

        static IEmitTypeCacheMethods()
        {
            TypeEmited_TypeMI = CacheType.LookupMIOrFail("TypeEmited", typeof(Type));

            GetPrinter_OpenT = CacheType.GetMethods().Where(
                    x => x.Name == "GetPrinter" &&
                    x.IsGenericMethod == true)
                .First();
        }

        /// <summary>
        /// Returns the closed generic <see cref="MethodInfo"/> object that represents the 
        /// function <see cref="IEmitTypeCache.GetPrinter{T}"/>
        /// </summary>
        /// <param name="T">The type to close the generic over</param>
        /// <returns> the <see cref="MethodInfo"/> object that represents the closed 
        /// generic type of <see cref="IEmitTypeCache.GetPrinter{T}"/> </returns>
        public static MethodInfo GetPrinter_ClosedT(Type T)
        {
            return GetPrinter_OpenT.MakeGenericMethod(T);
        }
    }
}
