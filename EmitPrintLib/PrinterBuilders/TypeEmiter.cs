namespace EmitPrintLib.PrinterBuilders
{
    using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text;
    using System.Threading.Tasks;
    using OC = System.Reflection.Emit.OpCodes; 
    /// <summary>
    /// This class is resposible for creating the drived type and creating the methodbuilder it then calls the method builder 
    /// </summary>
    internal sealed class TypeEmiter
    {
        const TypeAttributes ClassAttribs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed ;        
        private readonly TypeBuilder _TypeBuilder;
        private readonly IEmitTypeCache _TypeCache;   
        private readonly Type _ObjectType;
        private readonly AppendFormatMethodEmiter _AppendFormatEmiter = null; 

        private TypeEmiter(ModuleBuilder ModuleBuilder,Type ObjectType, IEmitTypeCache TypeCache)
        {
            Guard.ArgumentNotNull(ModuleBuilder, "ModuleBuilder");
            Guard.ArgumentNotNull(TypeCache, "TypeCache");
            var openGenType = typeof(BasePrinter<>);
            var closedType = openGenType.MakeGenericType(ObjectType);
            _ObjectType = ObjectType; 
            _TypeBuilder = ModuleBuilder.DefineType(GetImplName(ObjectType), ClassAttribs, closedType); 
            _TypeCache = TypeCache;
            _AppendFormatEmiter = new AppendFormatMethodEmiter(_ObjectType, _TypeBuilder); 
        }
        /// <summary>
        /// Creates the type an then call the metod generators 
        /// </summary>
        private object CreateType()
        {
            OverloadAppendFormat();
            var typ = _TypeBuilder.CreateType();
            return Activator.CreateInstance(typ);
        }

        private void OverloadAppendFormat()
        {
            //Hide this in a factory 
            _AppendFormatEmiter.CreateMethod(); 
             
        }

        

        /// <summary>
        /// Static method created the type and passes the type builder to the nested class 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="theModlue"></param>
        /// <param name="theCache"></param>
        /// <returns></returns>
        internal static IPrinter<T> Emit<T>(ModuleBuilder theModlue, IEmitTypeCache theCache)
        {   
            Type InterfaceType = typeof(IPrinter<T>);                        
            var Emiter = new TypeEmiter(theModlue, typeof(T), theCache);
            return (IPrinter<T>) Emiter.CreateType(); 
            
        }


        internal static string GetImplName(Type TheType)
        {
            return "Iprinter_" + TheType.Name + "_impl";
        }




        
        
    }
}
