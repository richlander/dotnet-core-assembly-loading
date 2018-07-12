using System;
using System.Reflection;
using System.Runtime.Loader;

public class ComponentHost : AssemblyLoadContext
{
    protected override Assembly Load(AssemblyName assemblyName)
    {
        return Assembly.Load(assemblyName);
    }
}