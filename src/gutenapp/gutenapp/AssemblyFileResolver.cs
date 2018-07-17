using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

public class AssemblyFileResolver
{

    private const string DEBUG = "debug";
    private const string RELEASE = "release";
    private const string BIN = "bin";
    private const string DLL = ".dll";
    private readonly string[] TARGETS = new string[] {"netcore", "netstandard"};
    private string _appTFM = string.Empty;

    public (bool libraryFound, string assemblyPath) GetColocatedLibraryForAssembly(string libraryName, Assembly assembly)
    {
        if (string.IsNullOrWhiteSpace(libraryName) || assembly ==  null)
        {
            throw new ArgumentException();
        }

        var assemblyDir = new DirectoryInfo(assembly.CodeBase);
        return ProbeDirectoryForLibrary(assemblyDir,libraryName);
    }

    public (bool libraryFound, string assemblyPath, string[] candidateLibraries) GetComponentLibrary(string libraryName)
    {
        // This policy enables loading add-ins that are not directly
        // colocated with the app (which is common).
        // In scenarios where assemblies are colocated with app binaries
        // use the GetColocatedLibraryForAssembly method

        // This code satisfies the following layouts:
        // App and libraries are located in peer directories
        // Libraries are subdirectories of app

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

        // The following directory structure demonstrates an expected
        // directory layout for prod layouts.
        // Looking up from app.exe:
        // app.exe (appbase is here)
        // app
        // Looking down from app dir:
        // app
        // component
        // component.dll (component that we want is here)


        // In the case that release or debug *app* directories are 
        // discovered that build kind is preferred for probing

        var extension = Path.GetExtension(libraryName);
        var componentName = string.IsNullOrEmpty(extension) ? libraryName : Path.GetFileNameWithoutExtension(libraryName);
        var library = string.IsNullOrEmpty(extension) ? libraryName + DLL : libraryName;
        var buildKind = string.Empty;
        DirectoryInfo probingDir;
        var candidateLibraries = new List<string>();

        // Expected location, per baseDir: same as AppContext.BaseDirectory
        probingDir = new DirectoryInfo(AppContext.BaseDirectory);

        var netDirFound = probingDir.Name.StartsWith("net");

        // Assumed to be prod-style deployment structure
        if (!netDirFound)
        {
            // Expected location, per appDir: app
            var (libraryFound, libraryPath) = ProbeComponentDirectoryForLibrary(probingDir,componentName, library);
            return (libraryFound, libraryPath,Array.Empty<string>());
        }
    
        // Expected location, per probingDir: app/bin/[buildKind]/tfm
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
                // Assumption: This is a development-time scenario
                // components are in peer directories
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
        var (componentDirFound, componentDir) = TryNavigateDirectoryDown(probingDir, componentName);

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
            return (false, string.Empty, Array.Empty<string>());
        }
    }

    private (bool libraryFound, string libraryPath, string[] candidateLibraries) ProbeForLibrariesInTargetFrameworkDirectories(DirectoryInfo probingDir, string library)
    {
        var candidateLibs = new List<string>();
        foreach (var target in TARGETS)
        {
            var dirs = probingDir.GetDirectories($"{target}*");
            foreach (var dir in dirs)
            {
                var (libraryFound, libraryPath) = ProbeDirectoryForLibrary(dir, library);
                if (libraryFound)
                {
                    candidateLibs.Add(libraryPath);
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

    private (bool libraryFound, string libraryPath) ProbeDirectoryForLibrary(DirectoryInfo probingDir, string library)
    {
        var fileList = probingDir.GetFiles(library);
        if (fileList.Length != 1)
        {
            return (false, string.Empty);
        }
        return (true, fileList[0].FullName);
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

    private (bool libraryFound, string libraryPath) ProbeComponentDirectoryForLibrary(DirectoryInfo probingDir, string componentName, string library)
    {
        var (componentDirFound, componentDir) = TryNavigateDirectoryDown(probingDir, componentName);

        if (componentDirFound)
        {
            var (libraryFound, libraryPath) = ProbeDirectoryForLibrary(componentDir, library);

            if (libraryFound)
            {
                return (libraryFound, libraryPath);
            }
            return False();
        }
        return False();

        (bool, string) False()
        {
            return (false, string.Empty);
        }
    }
}
