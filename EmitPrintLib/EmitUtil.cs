namespace EmitPrintLib
{    
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using Microsoft.Practices.EnterpriseLibrary.Common.Utility;

    /// <summary>
    /// Utility functions to help with code generation
    /// </summary>
    public static class EmitUtil
    {
        /// <summary>
        /// Lookups the <see cref="MethodInfo"/> for a function
        /// </summary>
        /// <param name="T">The Type to lookup </param>
        /// <param name="Name">The name of  the function</param>
        /// <returns><see cref="MethodInfo"/> or null if not found</returns>
        public static MethodInfo LookupMI(this Type T, string Name)
        {
            return LookupMI(T, Name, null); 
        }

        /// <summary>
        /// Lookups the <see cref="MethodInfo"/>
        /// </summary>
        /// <param name="T">The type that contains the function</param>
        /// <param name="Name">The name  of the function</param>
        /// <param name="Args">The arguments that the function takes</param>
        /// <returns><see cref="MethodInfo"/> or null if not found</returns>
        public static MethodInfo LookupMI(this Type T, string Name, params Type[] Args)
        {
            Args = Args ?? Type.EmptyTypes; 
            var MI = T.GetMethod(Name, MethodBindings.PublicInst, null, Args, null);            
            return MI; 
        }

        /// <summary>
        /// Lookups the <see cref="MethodInfo"/> or throws and exception
        /// </summary>
        /// <param name="T">The type that contains the method</param>
        /// <param name="Name">The name of the method</param>
        /// <exception cref="InvalidOperationException">If the function
        /// is not fount</exception>
        /// <returns>The MethodInfo</returns>
        public static MethodInfo LookupMIOrFail(this Type T, string Name)
        {
            return LookupMIOrFail(T, Name, null);
        }

        /// <summary>
        /// Lookups the mi or throws an exception
        /// </summary>
        /// <param name="T">The Type that contains the method</param>
        /// <param name="Name">The name.</param>
        /// <param name="Args">The arguments.</param>
        /// <returns>The <see cref="MethodInfo"/> for the requested function</returns>
        /// <exception cref="System.InvalidOperationException">Can't Find method  + Name +  on type  + T.Name</exception>
        public static MethodInfo LookupMIOrFail(this Type T, string Name,  params Type[] Args)
        {
            Args = Args ?? Type.EmptyTypes;
            var MI = T.GetMethod(Name, MethodBindings.PublicInst, null, Args, null);            
            if (MI == null)
            {
                throw new InvalidOperationException("Can't Find method " + Name + " on type " + T.Name); 
            }

            return MI;
        }

        /// <summary>
        /// Writes the il to call <see cref="Debug.WriteLine"/> method in the 
        /// generated function
        /// </summary>
        /// <param name="Generator">The <see cref="ILGenerator"/> that is currently
        /// emitting code</param>
        /// <param name="message">The Message to output</param>
        [Conditional("DEBUG")]
        public static void EmitDebugWriteLine(this ILGenerator Generator, string message)
        {
            Guard.ArgumentNotNull(Generator, "Generator");
            Guard.ArgumentNotNullOrEmpty(message, "message");
            
            var Fn = typeof(Debug).GetMethod("WriteLine", new Type[] { typeof(string) });
            Debug.Assert(Fn != null, "The write line method was not found");

            Generator.Emit(OpCodes.Ldstr, message);
            Generator.Emit(OpCodes.Call, Fn);
        }

        /// <summary>
        /// Prints the il position.
        /// </summary>
        /// <param name="Gen">The gen.</param>
        /// <param name="Message">The message.</param>
        [Conditional("DEBUG")]
        public static void PrintILPosition(this ILGenerator Gen, string Message)
        {
            Debug.WriteLine("{0} at offset = {1:X8} ", Message, Gen.ILOffset); 
        }

        /// <summary>
        /// Lookup to see if the passed in object declares and override of ToString
        /// </summary>
        /// <param name="theType">The type to check for a ToString</param>
        /// <returns>true if the object defines it's own ToString other wise false</returns>
        public static bool HasToStringOverload(Type theType)
        {
            return theType.GetMethod("ToString", EmitUtil.MethodBindings.PublicInstDeclaredOnly, null, Type.EmptyTypes, null) != null;
        }

        /// <summary>
        /// A collection of helper <see cref="BindingFlags"/> constants
        /// </summary>
        public static class MethodBindings
        {
            /// <summary>
            /// <see cref="BindingFlags"/> for Non public instance methods
            /// </summary>
            public const BindingFlags NonPublicInst = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic;

            /// <summary>
            /// <see cref="BindingFlags"/> for public instance methods 
            /// </summary>
            public const BindingFlags PublicInst = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public;

            /// <summary>
            /// Binding flags for public static methods 
            /// </summary>
            public const BindingFlags PublicStatic = BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public;

            /// <summary>
            /// <see cref="BindingFlags"/> for Non Public static methods
            /// </summary>
            public const BindingFlags NonPublicStatic = BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic;
            
            /// <summary>
            /// <see cref="BindingFlags"/> for non derived public methods
            /// </summary>
            public const BindingFlags PublicInstDeclaredOnly = PublicInst | BindingFlags.DeclaredOnly;
        }
    }
}
