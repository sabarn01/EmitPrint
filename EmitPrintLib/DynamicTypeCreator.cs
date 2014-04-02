namespace EmitPrintLib
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Class abstracts creating assemblies
    /// </summary>
    internal class AsseblyBuilderUtil 
    {
        public readonly ModuleBuilder MainModlue;
        private const TypeAttributes NormalTypeAttribs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
        private string AsemblysName = string.Empty;
        private AssemblyBuilder AsmBuilder = null;
        private List<TypeBuilder> CreatedTypes = new List<TypeBuilder>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AsseblyBuilderUtil"/> class. 
        /// </summary>
        /// <param name="assemblyName">The name of the Assembly to create</param>
        public AsseblyBuilderUtil(string assemblyName)
        {
            this.AsemblysName = assemblyName;
            var AsmName = new AssemblyName(assemblyName);
            AsmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AsmName, AssemblyBuilderAccess.RunAndSave);

            MainModlue = AsmBuilder.DefineDynamicModule(assemblyName, AssemblyFileName);
        }

        private string AssemblyFileName
        {
            get
            {
                return AsemblysName + ".dll";
            }
        }
      
        public void Save()
        {
            AsmBuilder.Save(AssemblyFileName);
        }
    }
}
