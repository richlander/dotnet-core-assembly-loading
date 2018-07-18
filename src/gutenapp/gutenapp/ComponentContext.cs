using System;
using System.Reflection;
using System.Runtime.Loader;

namespace ComponentHost
{
    public class ComponentContext : AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }

        public static (ComponentContext, Assembly) CreateContext(string assemblyPath)
        {
            var host = new ComponentContext();
            var assembly = host.LoadFromAssemblyPath(assemblyPath);
            return (host, assembly);
        }

        public static (ComponentContext, Assembly, T) CreateContext<T>(string assemblyPath, string typeName)
        {
            var (ComponentContext, assembly) = CreateContext(assemblyPath);
            T obj = (T)assembly.CreateInstance(typeName);
            return (ComponentContext, assembly, obj);
        }
    }
}
