using System;
using System.Reflection;
using System.Runtime.Loader;
using static System.Console;

namespace loaderexample
{
    class Program
    {
        static void Main(string[] args)
        {
            var assemblyLocation = "/Users/rlander/git/dotnet-core-assembly-loading/src/lib/out/lib.dll";
            WriteLine("Load lib.dll");
            var host = new ComponentHost();
            var assembly = host.LoadFromAssemblyPath(assemblyLocation);
            var type = assembly.GetType("lib.Class1");
            WriteLine("Call GetString method:");
            var method = type.GetMethod("GetString");
            var returnValue = method.Invoke(null, null);
            WriteLine(returnValue);

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // only display assemblies not loaded in the Default AssemblyLoadContext
                if (AssemblyLoadContext.GetLoadContext(asm) == AssemblyLoadContext.Default)
                {
                    continue;
                }

                WriteLine(asm.FullName);
            }
        }
    }
}
