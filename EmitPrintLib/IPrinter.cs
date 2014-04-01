using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitPrintLib
{
    
    internal interface IPrinterConfiguration
    {
        IReadOnlyCollection<TypeOverride> Overides {get;}
        void AddOverride<T>(IPrinter<T> OverridePrinter);
        void RemoveOveride(Type T);
    }

    public abstract class  BasePrinter<T> : IPrinter<T>
    {
        public virtual string FormatObject(object obj,IEmitTypeCache cache)
        {
            StringBuilder SB = new StringBuilder();
            SB.Append("{");
            if(obj == null)
            {
                SB.Append(" NULL ");
            }
            else
            {
                AppendFormat((T)obj, SB, cache);
            }
            
            SB.Append("}");
            return SB.ToString(); 

        }

        public abstract void AppendFormat(T obj, StringBuilder sb,IEmitTypeCache cache);        
    }

    public interface IPrinter
    {
        string FormatObject(object ob, IEmitTypeCache cache);          
    }
    public interface  IPrinter<T> : IPrinter
    {     
        void AppendFormat(T obj, StringBuilder sb, IEmitTypeCache cache);
    }    
}
