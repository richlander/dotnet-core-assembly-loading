using System;
using System.Collections.Generic;
using System.IO;
using ComponentHost;

namespace ComponentResolverStrategies
{
    public class ComponentResolverBinDirectoryStrategy : ComponentResolver
    {

        // The following directory structure demonstrates an expected
        // directory layout during development.
        // Looking up from app.exe:
        // app.exe (appbase is here)
        // netcoreapp2.1
        // debug | release
        // bin
        // app
        // Looking down from app dir:
        // app
        // component
        // bin
        // debug | release
        // netcoreapp2.1
        // component.dll (component that we want is here)

        // In the case that release or debug *app* directories are 
        // discovered that build kind is preferred for probing

        private const string DEBUG = "debug";
        private const string RELEASE = "release";
        private const string BIN = "bin";
        private const string DLL = ".dll";
        private readonly string[] TARGETS = new string[] { "netcore", "netstandard" };
        private string _appTFM = string.Empty;
        private string _buildKind = string.Empty;

        public override ComponentResolution FindLibrary(string library)
        {
            if (string.IsNullOrWhiteSpace(library))
            {
                throw new ArgumentException("libraryName not set");
            }
            return FindLibraryInComponentBinDirectory(library);
        }

        public override bool SetComponent(string component)
        {
            var (componentDirectoryFound, componentDirectory) = TryFindComponentDirectory(component);
            if (!componentDirectoryFound)
            {
                return false;
            }
            Component = component;
            BaseDirectory = componentDirectory;
            return true;
        }
        
        private (bool componentDirectoryFound, DirectoryInfo componentDirectory) TryFindComponentDirectory(string component)
        {
            // Expected location, per probingDir: app/bin/[buildKind]/tfm
            var probingDir = BaseDirectory;

            var count = 0;
            var dirName = string.Empty;
            while (count++ < 4)
            {
                probingDir = probingDir?.Parent;

                if (probingDir == null)
                {
                    return False();
                }
                
                dirName = probingDir.Name.ToLowerInvariant();

                if (dirName == DEBUG ||
                    dirName == RELEASE)
                {
                    _buildKind = dirName;
                }
                else if (dirName == BIN)
                {
                    // Assumption: components are in peer directories
                    // one level above bin
                    probingDir = probingDir.Parent?.Parent;
                    if (probingDir == null)
                    {
                        return False();
                    }
                    break;
                }
                else
                {
                    break;
                }
            }

            // Expected location, per probingDir: app
            var (componentDirFound, componentDir) = probingDir.TryNavigateDirectoryDown(component);

            if (!componentDirFound)
            {
                return False();
            }

            return (true, componentDir);

            (bool, DirectoryInfo) False()
            {
                return (false, BaseDirectory);
            }
        }
        
        private ComponentResolution FindLibraryInComponentBinDirectory(string library)
        {
            var candidateLibraries = new List<string>();

            // Expected location, per probingDir: app/component
            var probingDir = BaseDirectory;

            var (binFound, binDir) = probingDir.TryNavigateDirectoryDown(BIN);

            if (!binFound)
            {
                return False();
            }

            // Expected location, per probingDir: app/component/bin
            probingDir = binDir;

            DirectoryInfo releaseDir = null;
            DirectoryInfo debugDir = null;

            // these directories do not have uniform casing, hence foreach
            foreach (var dir in probingDir.GetDirectories())
            {
                var lowerName = dir.Name.ToLowerInvariant();
                if (lowerName == RELEASE)
                {
                    releaseDir = dir;
                }
                else if (lowerName == DEBUG)
                {
                    debugDir = dir;
                }
            }

            // Expected location, per probingDir: app/component/bin/[release | debug]
            if (releaseDir == null && debugDir == null)
            {
                return False();
            }

            // Probe release and debug directories
            // prefering release
            if (_buildKind == RELEASE && releaseDir != null)
            {
                probingDir = releaseDir;
            }
            else if (_buildKind == DEBUG && debugDir != null)
            {
                probingDir = debugDir;
            }
            else if (releaseDir != null)
            {
                probingDir = releaseDir;
            }
            else if (debugDir != null)
            {
                probingDir = debugDir;
            }
            else
            {
                return False();
            }

            return ProbeForLibrariesInTargetFrameworkDirectories(probingDir, library);

            ComponentResolution False()
            {
                var resolution = new ComponentResolution();
                resolution.ResolvedComponent = false;
                return resolution;
            }
        }

        private ComponentResolution ProbeForLibrariesInTargetFrameworkDirectories(DirectoryInfo probingDir, string library)
        {
            var resolution = new ComponentResolution();
            var candidateLibs = new List<string>();

            foreach (var target in TARGETS)
            {
                var dirs = probingDir.GetDirectories($"{target}*");
                foreach (var dir in dirs)
                {
                    var probeResult = ProbeDirectoryForLibrary(dir, library);
                    if (probeResult.ResolvedComponent)
                    {
                        candidateLibs.Add(probeResult.ResolvedPath);
                    }
                }
            }

            if (candidateLibs.Count == 0)
            {
                resolution.ResolvedComponent = false;
            }
            else
            {
                resolution.ResolvedComponent = true;
                resolution.Candidates = candidateLibs.ToArray();
                resolution.ResolvedPath = resolution.Candidates[0];
            }

            return resolution;
        }
    }
}
