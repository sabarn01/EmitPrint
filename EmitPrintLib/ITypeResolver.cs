using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitPrintLib
{
    interface ITypeResolver
    {
        Type Resolve(Type theType);
    }
}
