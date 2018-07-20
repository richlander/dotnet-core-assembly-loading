using System;
using System.IO;
using ComponentHost;

// The following directory structure demonstrates an expected
// directory layout for prod layouts.
// Looking up from app.exe:
// app.exe (appbase is here)
// app
// Looking down from app dir:
// app
// component
// component.dll (component that we want is here)

namespace ComponentResolverStrategies
{
    public class ComponentResolverProductionStrategy : ComponentResolver
    {
       
        public override ComponentResolution FindLibrary(string library)
        {
            if (string.IsNullOrWhiteSpace(library))
            {
                throw new ArgumentException("libraryName not set");
            }

            var resolution = new ComponentResolution();

            var (componentDirFound, componentDir) = BaseDirectory.TryNavigateDirectoryDown(Component);

            if (!componentDirFound)
            {
                resolution.ResolvedComponent = false;
                return resolution;
            }

            return ProbeDirectoryForLibrary(componentDir, library);
        }

        public override bool SetComponent(string component)
        {
            var (componentDirectoryFound, componentDirectory) = BaseDirectory.TryNavigateDirectoryDown(component);

            if (!componentDirectoryFound)
            {
                return false;
            }

            Component = component;
            BaseDirectory = componentDirectory;
            return true;

        }
    }
}
