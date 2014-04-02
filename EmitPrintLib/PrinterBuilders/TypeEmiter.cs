namespace EmitPrintLib.PrinterBuilders
{    
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using Microsoft.Practices.EnterpriseLibrary.Common.Utility;

    /// <summary>
    /// This class is responsible for creating the derived type and creating the method 
    /// builder it then calls the method builder 
    /// </summary>
    internal sealed class TypeEmiter
    {
        private const TypeAttributes ClassAttribs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;
        private readonly AppendFormatGenerator _AppendFormatEmiter = null;
        private readonly Type _ObjectType;
        private readonly TypeBuilder _TypeBuilder;
        private readonly IEmitTypeCache _TypeCache;  
 
        private TypeEmiter(ModuleBuilder ModuleBuilder, Type ObjectType, IEmitTypeCache TypeCache)
        {
            Guard.ArgumentNotNull(ModuleBuilder, "ModuleBuilder");
            Guard.ArgumentNotNull(TypeCache, "TypeCache");
            var openGenType = typeof(BasePrinter<>);
            var closedType = openGenType.MakeGenericType(ObjectType);
            _ObjectType = ObjectType; 
            _TypeBuilder = ModuleBuilder.DefineType(GetImplName(ObjectType), ClassAttribs, closedType); 
            _TypeCache = TypeCache;
            _AppendFormatEmiter = new AppendFormatGenerator(_ObjectType, _TypeBuilder, ModuleBuilder, _TypeCache); 
        }

        /// <summary>
        /// Static method created the type and passes the type builder to the nested class 
        /// </summary>
        /// <typeparam name="T">The type for which a printer should be created</typeparam>
        /// <param name="theModlue">The module that the <see cref="IPrinter&lt;T&gt;"/> </param>
        /// <param name="theCache">The cache of generated printers</param>
        /// <returns>the <see cref="IPrinter{T}"/> </returns>
        internal static IPrinter<T> Emit<T>(ModuleBuilder theModlue, IEmitTypeCache theCache)
        {
            Type InterfaceType = typeof(IPrinter<T>);
            var Emiter = new TypeEmiter(theModlue, typeof(T), theCache);
            return (IPrinter<T>)Emiter.CreateType();
        }

        internal static string GetImplName(Type TheType)
        {
            return "Iprinter_" + TheType.Name + "_impl";
        }

        /// <summary>
        /// Creates the type an then call the method generators 
        /// </summary>
        /// <returns>The created IPrinter&lt;T&gt;</returns>
        private object CreateType()
        {
            OverloadAppendFormat();
            var typ = _TypeBuilder.CreateType();
            return Activator.CreateInstance(typ);
        }

        private void OverloadAppendFormat()
        {
            // ToDo: Use a factory here 
            _AppendFormatEmiter.CreateMethod();              
        }
    }
}
