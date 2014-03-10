using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit; 

namespace EmitPrintLib
{    
    public interface MethodEmiter<TDelType> 
    {
        TDelType CreateDelegate();
        MethodBuilder GetMethod();
    }
}
