namespace EmitPrintLib
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// The main abstraction for emit printer 
    /// </summary>
    /// <typeparam name="T">The type that this printer can format</typeparam>
    public interface IPrinter<T>
    {
        /// <summary>
        /// Appends the format.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="sb">The StringBuilder must not be null.</param>
        /// <param name="cache">The cache.</param>        
        void AppendFormat(T obj, StringBuilder sb, IEmitTypeCache cache);

        /// <summary>
        /// Formats the object.
        /// </summary>
        /// <param name="ob">The ob.</param>
        /// <param name="cache">The cache.</param>
        /// <returns>A string of the correct format</returns>        
        string FormatObject(T ob, IEmitTypeCache cache);
    }

    /// <summary>
    /// This is the base type that emit print library uses to override for generated types.
    /// </summary>
    /// <typeparam name="T">The type that this object formats</typeparam>
    public abstract class  BasePrinter<T> : IPrinter<T>
    {
        /// <summary>
        /// Method to be overridden by generated types 
        /// </summary>
        /// <param name="obj">The object to print</param>
        /// <param name="sb">The string builder that is used to append text to </param>
        /// <param name="cache">The type cache which contains already generated printers</param>
        public abstract void AppendFormat(T obj, StringBuilder sb, IEmitTypeCache cache);

        /// <summary>
        /// This function creates a <see cref="StringBuilder"/> object that is passed 
        /// to the <see cref="AppendFormat"/> method
        /// </summary>
        /// <param name="obj">the object to print</param>
        /// <param name="cache">the type cache</param>        
        /// <returns>A string of the proper format</returns>
        public virtual string FormatObject(T obj, IEmitTypeCache cache)
        {
            StringBuilder SB = new StringBuilder();
            SB.Append("{");
            if (obj == null)
            {
                SB.Append(" NULL ");
            }
            else
            {
                AppendFormat(obj, SB, cache);
            }
            
            SB.Append("}");
            return SB.ToString(); 
        }
    }
}
