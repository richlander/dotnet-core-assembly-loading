using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace ComponentHost
{
    public class ComponentResolver : IComponentResolver
    {
        public ComponentResolver()
        {
            BaseDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        }

        public virtual ComponentResolution FindLibrary(string library)
        {
            if (string.IsNullOrWhiteSpace(library))
            {
                throw new ArgumentException("libraryName not set");
            }

            return ProbeDirectoryForLibrary(BaseDirectory,library);
        }

        public DirectoryInfo BaseDirectory {get;set;}

        public string Component {get;set;}

        protected ComponentResolution ProbeDirectoryForLibrary(DirectoryInfo probingDir, string library)
        {
            var resolution = new ComponentResolution();
            resolution.RequestedLibrary = library;
            var fileList = probingDir.GetFiles(library);
            if (fileList.Length != 1)
            {
                resolution.ResolvedLibrary = false;
                return resolution;
            }
            resolution.ResolvedLibrary = true;
            resolution.ResolvedLibraryPath = fileList[0].FullName;
            return resolution;
        }
    }
}
