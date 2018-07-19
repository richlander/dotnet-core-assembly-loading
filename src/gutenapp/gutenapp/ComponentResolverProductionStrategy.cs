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
        public ComponentResolverProductionStrategy(string component)
        {
            BaseDirectory = new DirectoryInfo(AppContext.BaseDirectory);
            Component = component;
        }
        
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
                resolution.ResolvedLibrary = false;
                return resolution;
            }

            return ProbeDirectoryForLibrary(componentDir, library);
        }

        public bool SetComponentDirectory(string directory)
        {
            throw new System.NotImplementedException();
        }
    }
}
