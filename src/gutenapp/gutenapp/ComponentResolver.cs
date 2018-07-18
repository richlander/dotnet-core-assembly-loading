using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace ComponentHost
{
    public class ComponentResolver
    {
        private const string DEBUG = "debug";
        private const string RELEASE = "release";
        private const string BIN = "bin";
        private const string DLL = ".dll";
        private readonly string[] TARGETS = new string[] {"netcore", "netstandard"};
        private string _appTFM = string.Empty;
        private DirectoryInfo _baseDirectory;

        public void SetBaseDirectory(string path)
        {
            SetBaseDirectory(new DirectoryInfo(path));
        }

        public void SetBaseDirectory(DirectoryInfo path)
        {
            _baseDirectory = path;
        }

        public void SetBaseDirectory(Assembly assembly)
        {
            SetBaseDirectory(new DirectoryInfo(assembly.CodeBase));
        }

        private void Check(string library)
        {
            if (string.IsNullOrWhiteSpace(library))
            {
                throw new ArgumentException("libraryName not set");
            }
            else if (_baseDirectory == null)
            {
                throw new ArgumentException("Base Directory not set");
            }
        }

        public ComponentResolution resolution FindLibrary(string library)
        {

            Check(library);
            return ProbeDirectoryForLibrary(_baseDirectory,library);
        }

        public ComponentResolution resolution FindComponentLibraryReleaseStrategy(string component, string library)
        {
            var libraryResolution = FindLibraryInComponentDirectory(component,library);

            if (libraryFound)
            {
                var resolution = new ComponentResolution();
                resolution 

                return Assemb
            }

            (libraryFound, libraryPath, var candidates) = FindLibraryInComponentBinDirectory(component, library);
        }

        public (bool libraryFound, string libraryPath) FindLibraryInComponentDirectory(string component, string library)
        {
            // The following directory structure demonstrates an expected
            // directory layout for prod layouts.
            // Looking up from app.exe:
            // app.exe (appbase is here)
            // app
            // Looking down from app dir:
            // app
            // component
            // component.dll (component that we want is here)


            Check(library);
            var probingDir = _baseDirectory;
            var (componentDirFound, componentDir) = TryNavigateDirectoryDown(probingDir, component);

            if (!componentDirFound)
            {
                return False();
            }
            
            var (libraryFound, libraryPath) = ProbeDirectoryForLibrary(componentDir, library);

            if (!libraryFound)
            {
                return False();
            }
            
            return (libraryFound, libraryPath);

            (bool, string) False()
            {
                return (false, string.Empty);
            }
        }

        public ComponentResolution FindLibraryInComponentBinDirectory(string component, string library)
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

            Check(library);
            var buildKind = string.Empty;
            var candidateLibraries = new List<string>();

            // Expected location, per probingDir: app/bin/[buildKind]/tfm
            var probingDir = _baseDirectory;

            var count=0;
            var dirName = string.Empty;
            while(count++ < 4)
            {
                if (probingDir == null)
                {
                    return False();
                }
                probingDir = probingDir.Parent;
                dirName = probingDir.Name.ToLowerInvariant();

                if (dirName == DEBUG ||
                    dirName == RELEASE)
                {
                    buildKind = dirName;
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
            var (componentDirFound, componentDir) = TryNavigateDirectoryDown(probingDir, component);

            if (!componentDirFound)
            {
                return False();
            }

            // Expected location, per probingDir: app/component
            probingDir = componentDir;

            var (binFound, binDir) = TryNavigateDirectoryDown(probingDir, BIN);

            if (!binFound)
            {
                return False();
            }

            // Expected location, per probingDir: app/component/bin
            probingDir = binDir;

            DirectoryInfo releaseDir = null;
            DirectoryInfo debugDir = null;

            // these directories do not have uniform casing
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
            if (buildKind == RELEASE && releaseDir != null)
            {
                probingDir = releaseDir;

            }
            else if (buildKind == DEBUG && debugDir!= null)
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

            (bool, string, string[]) False()
            {
                var resolution = new ComponentResolution()
                {
                    Resolved = False;
                };
                return resolution;
            }
        }

        private ComponentResolution ProbeForLibrariesInTargetFrameworkDirectories(DirectoryInfo probingDir, string library)
        {
            var candidateLibs = new List<string>();
            foreach (var target in TARGETS)
            {
                var dirs = probingDir.GetDirectories($"{target}*");
                foreach (var dir in dirs)
                {
                    var resolution = ProbeDirectoryForLibrary(dir, library);
                    if (resolution.Resolved)
                    {
                        candidateLibs.Add(resolution.ResolvedPath);
                    }
                }
            }

            if (candidateLibs.Count > 0)
            {
                var libs = candidateLibs.ToArray();
                return (true, libs[0], libs);
            }
            return (false, string.Empty, Array.Empty<string>());
        }

        private ComponentResolution ProbeDirectoryForLibrary(DirectoryInfo probingDir, string library)
        {
            var resolution = new ComponentResolution();
            resolution.Library = library;
            var fileList = probingDir.GetFiles(library);
            if (fileList.Length != 1)
            {
                resolution.Resolved = false;
                return resolution;
            }
            resolution.Resolved = true;
            resolution.ResolvedPath = fileList[0].FullName;
            return resolution;
        }

        private (bool, DirectoryInfo) TryNavigateDirectoryUp(DirectoryInfo dir, string name)
        {
            if (dir.Parent?.Name == name)
            {
                return (true, dir.Parent);
            }
            return (false,dir);
        }

        private (bool, DirectoryInfo) TryNavigateDirectoryDown(DirectoryInfo dir, string name)
        {
            var list = dir.GetDirectories(name);
            if (list.Length == 1)
            {
                return (true, list[0]);
            }
            return (false, dir);
        }
    }
}
