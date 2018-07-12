using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using static System.Console;

namespace loadlibrary
{
    class Program
    {
        static void Main(string[] args)
        {
            var assemblyLocation = "/Users/rlander/git/dotnet-core-assembly-loading/src/lib/out/lib.dll";
            WriteLine("Load lib.dll");
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyLocation);
            var type = assembly.GetType("lib.Class1");
            WriteLine("Call GetString method:");
            var method = type.GetMethod("GetString");
            var returnValue = method.Invoke(null,null);
            WriteLine(returnValue);
            WriteLine("Print loaded (non-platform) assemblies:");
            foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.FullName.StartsWith("System") || asm.FullName.StartsWith("netstandard") )
                {
                    continue;
                }

                WriteLine(asm.FullName);
            }
        }
    }
}
