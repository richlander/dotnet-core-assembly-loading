using System;
using System.Reflection;
using System.Runtime.Loader;

namespace loaderexample
{
    public class Class1
    {
        public static string GetString()
        {
            return "Bow ties are cool.";
        }

        public static string GetOtherString()
        {
            var assemblyLocation = "/Users/rlander/git/dotnet-core-assembly-loading/src/addin-loadassemblybypath/lib2/bin/Debug/netstandard2.0/lib2.dll";
            var context = AssemblyLoadContext.GetLoadContext(typeof(Class1).Assembly);
            var assembly = context.LoadFromAssemblyPath(assemblyLocation);
            var type = assembly.GetType("Class2");
            var method = type.GetMethod("GetOtherString");
            var returnValue = (string)method.Invoke(null, null);
            return returnValue;
        }

    }
}
