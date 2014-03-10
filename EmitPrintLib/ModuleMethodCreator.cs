using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EmitPrintLib
{
    class ModuleMethodCreator : IMethodCreator
    {
        public dynamic CreateMethod(string Name, Type Ret, Type[] Parmeters)
        {
            return new DynamicMethod(Name, Ret, Parmeters);
        }


        public void SealMethod()
        {
            
        }
    }
}
