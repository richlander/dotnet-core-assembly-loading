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
            
            var assemblyLocation = "/Users/rlander/git/dotnet-core-assembly-loading/src/addin-loadassemblybypath/lib/bin/Debug/netcoreapp2.1/lib.dll";
            WriteLine("Load lib.dll");
            var host = new ComponentHost();
            var assembly = host.LoadFromAssemblyPath(assemblyLocation);
            var type = assembly.GetType("loaderexample.Class1");
            WriteLine("Call GetString method:");
            var method = type.GetMethod("GetString");
            var returnValue = method.Invoke(null, null);
            WriteLine(returnValue);
            WriteLine("Print loaded assemblies in 'host' AssemblyLoadContext:");
            WriteLine("Call GetOtherString method:");
            var method2 = type.GetMethod("GetOtherString");
            var returnValue2 = method2.Invoke(null, null);
            WriteLine(returnValue2);
            WriteLine("Print loaded assemblies in 'host' AssemblyLoadContext:");

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // only display assemblies not loaded in the Default AssemblyLoadContext
                if (AssemblyLoadContext.GetLoadContext(asm) == host)
                {
                    WriteLine(asm.FullName);
                }
            }
        }
    }
}
