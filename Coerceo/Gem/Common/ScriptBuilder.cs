using System;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Gem
{
    public class ScriptBuilder
    {
        private CodeDomProvider CodeProvider = CodeDomProvider.CreateProvider("CSharp");
        private CompilerParameters CompilerParameters = null;
        private Action<String> ReportErrors = null;

        private String UsingBlock = @"using System;
using System.Collections.Generic;
using System.Linq;
";
        private String TypeHeader = @"class CSharpScript {
    public static void Execute() {
";

        private String TypeFooter = @"
    }
}";

        public ScriptBuilder()
        {
            PrepareCompilerParameters();
        }

        public ScriptBuilder(Action<String> ReportErrors)
        {
            this.ReportErrors = ReportErrors;
            PrepareCompilerParameters();
        }

        public void DeriveScriptsFrom(String SuperClass)
        {
            TypeHeader = @"class CSharpScript : " + SuperClass + @"{
    public static void Execute() {
";
        }

        private void PrepareCompilerParameters()
        {
            if (CompilerParameters != null) return;

            CompilerParameters = new CompilerParameters();
            CompilerParameters.GenerateExecutable = false;
            CompilerParameters.GenerateInMemory = true;

            CompilerParameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            CompilerParameters.ReferencedAssemblies.Add("mscorlib.dll");
            CompilerParameters.ReferencedAssemblies.Add("System.dll");
            CompilerParameters.ReferencedAssemblies.Add("System.Core.dll");
            CompilerParameters.ReferencedAssemblies.Add("System.Data.Linq.dll");
        }

        public void AddReference(String AssemblyName, String AssemblyPath)
        {
            if (!String.IsNullOrEmpty(AssemblyName))
                UsingBlock += "using " + AssemblyName + ";\n";
            if (!String.IsNullOrEmpty(AssemblyPath))
                CompilerParameters.ReferencedAssemblies.Add(AssemblyPath);
        }

        /// <summary>
        /// Compile a complete, well formed file.
        /// </summary>
        /// <param name="FileContents"></param>
        /// <returns>The assembly generated from the source</returns>
        public Assembly CompileFile(String FileContents)
        {
            var compilationResults = CodeProvider.CompileAssemblyFromSource(CompilerParameters, FileContents);

            if (compilationResults.Errors.Count > 0 && ReportErrors != null)
            {
                foreach (var error in compilationResults.Errors)
                    ReportErrors((error as CompilerError).ErrorText);
                return null;
            }
            else
                return compilationResults.CompiledAssembly;
        }

        /// <summary>
        /// Compile a snippet. The snippet is embedded in a function that returns nothing.
        /// </summary>
        /// <param name="Script"></param>
        /// <returns>An action that invokes the compiled snippet</returns>
        public Action CompileScript(String Script)
        {
            var fileContents = UsingBlock + TypeHeader + Script + TypeFooter;
            var assembly = CompileFile(fileContents);

            if (assembly != null)
            {
                var scriptType = assembly.GetType("CSharpScript");
                if (scriptType == null) throw new InvalidOperationException("CSharpScript wasn't found in the generated assembly. This should be impossible.");
                var function = scriptType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);
                if (function == null) throw new InvalidOperationException("static void Execute wasn't found in the CSharpScript type. This should be impossible.");
                return new Action(() => function.Invoke(null, null));
            }
            else
                return null;
        }
    }
}
