using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EmitPrintLib
{
    interface IMethodCreator
    {
        dynamic CreateMethod(string Name,Type Ret,Type[] Parmeters);
        void SealMethod(); 
    }
}
