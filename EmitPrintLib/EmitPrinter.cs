namespace EmitPrintLib
{
    using System.Text;
    using EmitPrintLib.PrinterBuilders;
    
    /// <summary>
    /// <see cref="EmitPrinter"/> is a builder for the emit print library 
    /// it creates the necessary types to create a printer. 
    /// </summary>
    public class EmitPrinter
    {
        private readonly AsseblyBuilderUtil _asmUtil = null;
        private readonly IEmitTypeCache _typeCache = null; 

        /// <summary>
        /// Initializes a new instance of the <see cref="EmitPrinter"/> class
        /// </summary>
        /// <param name="AssemblyName">The Name of the assembly to create</param>
        public EmitPrinter(string AssemblyName) :
            this(AssemblyName, new EmitTypeCache())
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmitPrinter"/> class
        /// </summary>
        /// <param name="AssemblyName">The name of the dynamic assembly to create</param>
        /// <param name="cache">The <see cref="IEmitTypeCache"/> which contains created 
        /// printers 
        /// </param>
        public EmitPrinter(string AssemblyName, IEmitTypeCache cache)            
        {
            _typeCache = cache;
            _asmUtil = new AsseblyBuilderUtil(AssemblyName);
        }

        /// <summary>
        /// Creates an <see cref="IPrinter{T}"/> and uses it to return a string.
        /// </summary>
        /// <typeparam name="T">The type to print</typeparam>
        /// <param name="obj">The object to print</param>
        /// <returns>A string created by the Printers</returns>
        public string EmitPrintMessage<T>(T obj)
        {
            var theType = typeof(T); 
            if (!_typeCache.TypeEmited(theType))
            {
                var emiter = TypeEmiter.Emit<T>(_asmUtil.MainModlue, _typeCache);
                _typeCache.AddPrinter(emiter); 
                _asmUtil.Save(); 
            }

            var emitPrinter = _typeCache.GetPrinter<T>();             
            if (emitPrinter != null)
            {
                return emitPrinter.FormatObject(obj, _typeCache);
            }
            else
            {
                return obj.ToString();
            }
        }           
    }
}
