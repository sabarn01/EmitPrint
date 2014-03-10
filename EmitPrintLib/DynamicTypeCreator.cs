using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EmitPrintLib
{
    class AsseblyBuilderUtil 
    {
        const TypeAttributes NormalTypeAttribs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
        List<TypeBuilder> CreatedTypes = new List<TypeBuilder>();
        public readonly ModuleBuilder MainModlue;
        string AsemblysName = "";
        string AssemblyFileName
        {
            get
            {
                return AsemblysName + ".dll"; 
            }
        }
        AssemblyBuilder AsmBuilder = null;
        public AsseblyBuilderUtil(string sAsmName)
        {
            this.AsemblysName = sAsmName;
            var AsmName = new AssemblyName(sAsmName);
            AsmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AsmName, AssemblyBuilderAccess.RunAndSave);

            MainModlue = AsmBuilder.DefineDynamicModule(sAsmName, AssemblyFileName);
        }
      
        public void Save()
        {
            AsmBuilder.Save(AssemblyFileName);
        }

   
    }
}
