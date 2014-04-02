namespace EmitPrintLib
{
    using System;
    using System.Collections.Generic;

    internal class EmitTypeCache : IEmitTypeCache
    {
        private readonly Dictionary<Type, object> _emitedPrinters = new Dictionary<Type, object>();

        public bool TypeEmited(Type T)
        {
            return _emitedPrinters.ContainsKey(T);
        }

        public IPrinter<T> GetPrinter<T>()
        {
            return (IPrinter<T>)_emitedPrinters[typeof(T)];
        }

        public void AddPrinter<T>(IPrinter<T> printer)
        {
            _emitedPrinters.Add(typeof(T), printer);
        }
    }
}
