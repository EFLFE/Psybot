using System;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using PsybotPlugin;

namespace Psybot.Plugins
{
    // for source code
    [Obsolete("Not ready.", true)]
    internal sealed class PluginCompiler
    {
        private CSharpCodeProvider codeProvider = null;
        private CompilerParameters parameters = null;

        public PluginCompiler()
        {
            codeProvider = new CSharpCodeProvider();
            parameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = true
            };
            // CompilerOptions: https://msdn.microsoft.com/ru-ru/library/6ds95cz0.aspx
            // anycpu, anycpu32bitpreferred, ARM, x64, x86, Itanium
            parameters.CompilerOptions
                = "/optimize "
#if LINUX
                + "/define:LINUX "
#else
                + "/define:WINDOWS "
#endif
#if X64
                + "/platform:x64 "
#else
                + "/platform:anycpu "
#endif
                //+ "win32icon:?.ico "
                + "/nowarn:1607 /nowarn:162"; // на время, что бы не отвлекал

            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("PsybotPlugin.dll");
        }

        [Obsolete("TERM XY?", true)]
        public IPsybotPlugin CompileSourceFiles(string[] files)
        {
            // BUILD
            Console.WriteLine("Compiling...");
            var result = codeProvider.CompileAssemblyFromFile(parameters, files);

            // show need errors
            if (result.Errors.Count > 0)
            {
                Console.WriteLine(string.Format("Need {0} errors:", result.Errors.Count));

                foreach (CompilerError ce in result.Errors)
                {
                    if (ce.IsWarning)
                    {
                        Console.WriteLine(string.Format("\nWarning {0}: {1}", ce.ErrorNumber, ce.ErrorText));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("\nError {0}: {1}", ce.ErrorNumber, ce.ErrorText));
                    }

                    if (ce.FileName.Length == 0 || !File.Exists(ce.FileName))
                        continue;

                    // show line text error
                    Console.WriteLine(string.Format("In line {0}, file: {1}", ce.Line, ce.FileName));
                    using (var sr = File.OpenText(ce.FileName))
                    {
                        var txt = string.Empty;
                        for (int i = 0; i < ce.Line; i++)
                        {
                            txt = sr.ReadLine();
                        }
                        Console.WriteLine(txt.Trim());
                    }
                }

                return null;
            }

            Console.WriteLine("Source built successfully!");

            var type = result.CompiledAssembly.GetType("TestPlugin.Baka");
            IPsybotPlugin typePlug = (IPsybotPlugin)Activator.CreateInstance(type);
            //typePlug.Load();

            Console.WriteLine("Create instance successfully!");

            return typePlug;
        }

        public void CompileCSPROJ()
        {
            // parse csproj?
        }

    }
}
