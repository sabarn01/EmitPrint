using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitPrintLib
{

    public delegate void PrinterNotFoundHandler(IEmitTypeCache sender, PrinterNotFoundEventArgs Args);
    public interface IEmitTypeCache
    {
        /// <summary>
        /// Any type passed into this interface will passed into type resolver first 
        /// </summary>
        //TypeResolver Resolver { get; set; }
        //event PrinterNotFoundHandler PrinterNotFound; 
        bool TypeEmited(Type T);
        /// <summary>
        /// Get Printer will called if the printer is not found for the requested type with 
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        IPrinter GetPrinter(Type T); 
        IPrinter<T> GetPrinter<T>(); 
        void AddPrinter<T>(IPrinter<T> printer);
    }
}
