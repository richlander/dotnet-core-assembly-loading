using System;
using System.Reflection;
using System.Runtime.Loader;

namespace ComponentHost
{
    public class ComponentContext : AssemblyLoadContext
    {

        private ComponentResolver[] _resolvers;

        public ComponentContext(string component, params ComponentResolver[] resolvers)
        {
            _resolvers = resolvers;
            Component = component;
        }

        private ComponentContext()
        {
        }

        public string Component {get; private set;}

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
                var componentResolution = resolver.SetComponent(Component);
                if (!componentResolution)
                {
                    continue;
                }
                var libraryResolution = resolver.FindLibrary(assemblyFile);
                if (libraryResolution.ResolvedComponent)
                {
                    return LoadFromAssemblyPath(libraryResolution.ResolvedPath);    
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
