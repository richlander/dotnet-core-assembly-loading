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

        public DirectoryInfo BaseDirectory { get; set; }

        public string Component { get; set; }

        public virtual bool SetComponent(string component)
        {
            Component = component;
            return true;
        }

        public virtual ComponentResolution FindLibrary(string library)
        {
            if (string.IsNullOrWhiteSpace(library))
            {
                throw new ArgumentException("libraryName not set");
            }

            return ProbeDirectoryForLibrary(BaseDirectory,library);
        }

        protected ComponentResolution ProbeDirectoryForLibrary(DirectoryInfo probingDir, string library)
        {
            var fileList = probingDir.GetFiles(library);
            if (fileList.Length != 1)
            {
                return GetFalseResolution(library);
            }
            var resolution = new ComponentResolution();
            resolution.RequestedComponent = library;
            resolution.ResolvedComponent = true;
            resolution.ResolvedPath = fileList[0].FullName;
            return resolution;
        }

        protected ComponentResolution GetFalseResolution(string component)
        {
            var resolution = new ComponentResolution();
            resolution.RequestedComponent = component;
            resolution.ResolvedComponent = false;
            return resolution;
        }
    }
}
