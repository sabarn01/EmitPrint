namespace EmitPrintLib
{
    using System;

    /// <summary>
    /// Interface represents a set of IPrinter&lt;T&gt;
    /// </summary>
    public interface IEmitTypeCache
    {
        /// <summary>
        /// Any type passed into this interface will passed into type resolver first 
        /// </summary>                
        /// <param name="T">The type of the Printer that is desired</param>
        /// <returns>if type printer for the requested type exists in the cache </returns>
        bool TypeEmited(Type T);

        /// <summary>
        /// Get Printer will called if the printer is not found for the requested type with 
        /// </summary>
        /// <typeparam name="T">The type of the emit printer to get</typeparam>
        /// <returns>true if the type is in the cache false other wise </returns>       
        IPrinter<T> GetPrinter<T>(); 

        /// <summary>
        /// Adds a printer to the cache.  It is undefined if calling add with the same type 
        /// overrides or throws an error. 
        /// </summary>
        /// <typeparam name="T">The type the printer formats</typeparam>
        /// <param name="printer">The printer to add</param>
        void AddPrinter<T>(IPrinter<T> printer);
    }
}
