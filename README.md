# Assembly Loading with .NET Core

Loading assemblies is an important part of many programs. In most cases, your static dependencies will be loaded automatically, but dynamic dependencies require active use of various assembly loader APIs. For any program that exposes an add-in model, assembly loading is a critical part of the application architecture.

Many .NET developers are familar with the .NET Framework assembly loading model, using [Assembly](https://docs.microsoft.com/dotnet/api/system.reflection.assembly?view=netframework-4.7.2) and [AppDomain](https://docs.microsoft.com/dotnet/api/system.appdomain?view=netframework-4.7.2) class APIs. .NET Core exposes a similar model, with differences. The biggest difference is that .NET Core exposes [AssemblyLoadContext](https://docs.microsoft.com/dotnet/api/system.runtime.loader.assemblyloadcontext), which has some overlap with AppDomain, but is a much lighter-weight subsystem.

Resources:

* [AssemblyLoadContext design document](https://github.com/dotnet/coreclr/blob/master/Documentation/design-docs/assemblyloadcontext.md)
* [AssemblyLoadContext source](https://github.com/dotnet/coreclr/blob/master/src/System.Private.CoreLib/src/System/Runtime/Loader/AssemblyLoadContext.cs)

This document describes a set of scenarios with guidance and samples for implementing those scenarios.

## AssemblyLoaderContext assembly loading model

You need to understand the basics of the new [AssemblyLoadContext](https://docs.microsoft.com/dotnet/api/system.runtime.loader.assemblyloadcontext) (ALC) type to correctly control assembly loading.

The basic function and value proposition of ALC is enabling assembly loading isolation within a process. This is similar to what the AppDomain type provides with .NET Framework. Within a given ALC, you can load an assembly with a given simple name, like foo.dll, just once. You can load assemblies with that same name multiple times across multiple ALCs. The version and publisher of the assembly does not need to match across ALCs. That statics for an assembly are unique (and don't leak) across ALCs.

Every assembly is loaded into an ALC. By default, assemblies are loaded into the default ALC. Assemblies that are loaded into the default ALC are visible to all other ALCs, including the values of static fields.

The [AssemblyLoadContext](https://docs.microsoft.com/dotnet/api/system.runtime.loader.assemblyloadcontext) class exposes a set of assembly loading APIs. These APIs, by virtue of being instance APIs, load assemblies into the current ALC. They can load by name, file, or stream.

The default ALC is available via the static `AssemblyLoadContext.Default` property. Non-default ALCs are available by using APIs that expose ALCs, which are discussed later. ALCs do not currently have names. You can only differentiate ALCs via equality comparisons, like comparing the default ALC to another ALC reference.

The [Assembly](https://docs.microsoft.com/dotnet/api/system.reflection.assembly?view=netcore-2.1) class exposes a set of assembly loading APIs, the same ones that are available in the .NET Framework. These APIs have the following behavior:

* Assembly.Load - loads assembly and its dependencies into the current ALC. One can consider this API neutral in nature, leaving all loading policy up to the host application.
* Assembly.LoadFrom - loads assembly and its dependencies into the default ALC. Only a application or host should use this API.
* Assembly.LoadFile and Assembly.Load(Byte[]) - loads assembly and its depenencies into a new ALC.

## Loading an assembly by path

The simplest scenario is loading an assembly by path. The following code loads an assembly by path into the default ALC.

```csharp
var assemblyLocation = "/fully/qualified-path/to.dll"
var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyLocation);
```

Alternatively, you can `Assembly` class APIs. `Assembly.LoadFrom` will load assemblies into the default ALC. `Assembly.LoadFile` will load assemblies into a new ALC with each invocation.

## Loading an assembly by path as an add-in

One challenge of the current ALC implementation is that there is no way for add-ins to load files by path without coordination with some kind of add-in host within the ALC. This seems too restrictive.

An [AssemblyLoadContext.Current](https://github.com/dotnet/coreclr/issues/10233) API would enable ALCs to load dependencies independent of its host. One challenge of that API is it would prevent hosts from building restrictive environments, where add-ins are not allowed to load additional assemblies. This option seems too open.

Custom ALCs must derive from AssemblyLoadContext. One can imagine a small set of ALC implementations becoming popular and enabling their own loading policies for add-ins. One can imagine a `ComponentHost` ALC implementation that exposing `ComponentHost.Current` to add-ins that it hosts, and `ComponentHostRestrictive` not doing that. One can also imagine a middle ground API that enables both scenarios, such as `ComponentHost.Current.RequestAssemblyLoadByPath()`. The problem with this approach is that an add-in gets tied to a specific ALC implementation. This seems too restrictive.

Another option is that 

