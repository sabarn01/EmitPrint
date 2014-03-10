using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmitPrintLib
{

    class EmitTypeCache : IEmitTypeCache
    {
        private readonly Dictionary<Type,IPrinter> _emitedPrinters = new Dictionary<Type,IPrinter>(); 
        
        public bool TypeEmited(Type T)
        {
 	        return _emitedPrinters.ContainsKey(T) ;
        }
        
        public IPrinter GetPrinter(Type T)
        {
            return _emitedPrinters[T]; 
        }

        public IPrinter<T> GetPrinter<T>()
        {
            return (IPrinter<T>)GetPrinter(typeof(T)); 
        }

        public void AddPrinter<T>(IPrinter<T> printer)
        {                         
            _emitedPrinters.Add(typeof(T),printer);
        }

    

    }
    
}
