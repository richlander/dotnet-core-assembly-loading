using System;
using System.Reflection;
using System.Runtime.Loader;

namespace ComponentHost
{
    public class ComponentContext : AssemblyLoadContext
    {

        private ComponentResolver[] _resolvers;

        public ComponentContext(params ComponentResolver[] resolvers)
        {
            _resolvers = resolvers;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }

        public Assembly LoadAssemblyWithResolver(string assemblyFile)
        {
            if (_resolvers == null)
            {
                return null;
            }

            foreach (var resolver in _resolvers)
            {
                var componentResolution = resolver.FindLibrary(assemblyFile);
                if (componentResolution.ResolvedLibrary)
                {
                    return LoadFromAssemblyPath(componentResolution.ResolvedLibraryPath);
                }

            }

            return null;
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
