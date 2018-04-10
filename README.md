[![Chat on Gitter](https://img.shields.io/gitter/room/fody/fody.svg?style=flat&max-age=86400)](https://gitter.im/Fody/Fody)
[![NuGet Status](http://img.shields.io/nuget/v/BasicFodyAddin.Fody.svg?style=flat&max-age=86400)](https://www.nuget.org/packages/BasicFodyAddin.Fody/)

![Icon](https://raw.githubusercontent.com/Fody/BasicFodyAddin/master/package_icon.png)

This is a simple solution built as a starter for writing [Fody](https://github.com/Fody/Fody) addins.


## Usage

See also [Fody usage](https://github.com/Fody/Fody#usage).


### NuGet installation

Install the [BasicFodyAddin.Fody NuGet package](https://nuget.org/packages/BasicFodyAddin.Fody/) and update the [Fody NuGet package](https://nuget.org/packages/Fody/):

```
PM> Install-Package BasicFodyAddin.Fody
PM> Update-Package Fody
```

The `Update-Package Fody` is required since NuGet always defaults to the oldest, and most buggy, version of any dependency.


### Add to FodyWeavers.xml

Add `<BasicFodyAddin/>` to [FodyWeavers.xml](https://github.com/Fody/Fody#add-fodyweaversxml)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Weavers>
  <BasicFodyAddin/>
</Weavers>
```


## The moving parts


### BasicFodyAddin Project

A project that contains all classes used for compile time metadata. Generally any usage and reference to this is removed at compile time so it is not needed as part of application deployment.

This project is also used to produce the Nuget package. To achieve this the project consumes two NuGets:

 * [Fody](https://www.nuget.org/packages/Fody/) with `PrivateAssets="None"`. This results in produce NuGet package having a dependency on Fody with all ` include="All"` in the nuspec. Not that while this project consumes the Fody nuget, weaving is not performed on this project. This is dues to the FodyPackaging NuGet (see below) including ` <DisableFody>true</DisableFody>` in the msbuild pipeline.
 * [FodyPackaging](https://www.nuget.org/packages/FodyPackaging/) with `PrivateAssets="All"`. This results in the a NuGet being produced by this project, but no dependency on FodyPackaging in that  produced NuGet. 

The NuGet produced will be named after this project with the suffix `.Fody` added. 

This project should also contain all appropriate [NuGet metadata properties](https://docs.microsoft.com/en-us/dotnet/core/tools/csproj#nuget-metadata-properties). Many of these properties are [defaulted by FodyPackaging](https://github.com/Fody/Fody/blob/master/FodyPackaging/build/FodyPackaging.props) but can be overriden.

The resultant NuGet will taget the same frameworks that this project targets.

The resultant NuGet will be created in a diretory named `nugets` at the root of the solution.


### BasicFodyAddin.Fody Project

The project that does the weaving.

The project has a NuGet dependency on  [FodyHelpers](https://www.nuget.org/packages/FodyHelpers/) .


#### Target Frameworks

This project targets `net46` and `netstandard2.0` so that it can target `msbuild.exe` and `dotnet build` respectively.


#### Output of the project

It outputs a file named `BasicFodyAddin.Fody`. The '.Fody' suffix is necessary for it to be picked up by Fody at compile time.


#### ModuleWeaver

ModuleWeaver.cs is where the target assembly is modified. Fody will pick up this type during a its processing. Note that it must be named `ModuleWeaver`

`ModuleWeaver` has a base class of `BaseModuleWeaver` which exists in the [FodyHelpers NuGet](https://www.nuget.org/packages/FodyHelpers/).


##### BaseModuleWeaver.Execute

Called to perform the manipulation of the module. The current module can be accessed and manipulated via `BaseModuleWeaver.ModuleDefinition`.


##### BaseModuleWeaver.GetAssembliesForScanning

Called by fody when it is building up a type cache for lookups. This method should return all possible assemblies that the weaver may required for type resolution. In this case BasicFodyAddin requires `System.Object`, so `GetAssembliesForScanning` returns `netstandard` and `mscorlib`. It is safe to return assembly names that do not resolve for the runtime of current target assembly as these will be ignored.

To use this type cache a `ModuleWeaver` can call `BaseModuleWeaver.FindType` from within `Execute`. For example in this project the following is called:

```
var objectRef = ModuleDefinition.ImportReference(FindType("System.Object"));
```

##### BaseModuleWeaver.ShouldCleanReference

When `BasicFodyAddin.dll` is referenced by a consuming project, it is only for the purposes configuring the weaving via attributes. As such is it nor required at runtime. With this in mind `BaseModuleWeaver` has an opt in feature to remove the reference, meaning the target weaved application does not need `BasicFodyAddin.dll` at run time. This feature can be opted in to via the following.

```
public override bool ShouldCleanReference => true;
```


##### Other BaseModuleWeaver Members

`BaseModuleWeaver` has a number of other members for logging and extensibility:  
https://github.com/Fody/Fody/blob/master/FodyHelpers/BaseModuleWeaver.cs


#### Resultant injected code

In this case a new type is being injected into the target assembly that looks like this.

```
public class Hello
{
    public string World()
    {
        return "Hello World";
    }
}
```

See [ModuleWeaver](https://github.com/Fody/Fody/wiki/ModuleWeaver) for more details.


### AssemblyToProcess Project

A target assembly to process and then validate with unit tests.


### Tests Project

Contains all tests for the weaver.

The project has a NuGet dependency on [FodyHelpers](https://www.nuget.org/packages/FodyHelpers/) .

It has a reference to the `AssemblyToProcess` project, so that `AssemblyToProcess.dll` is copied to the bin directory of the test project.

FodyHelpers contains a utility [WeaverTestHelper](https://github.com/Fody/Fody/blob/master/FodyHelpers/Testing/WeaverTestHelper.cs) for executing test runs on a target assembly using a ModuleWeaver. 

A test can then be run as follows:

```
public class WeaverTests
{
    static TestResult testResult;

    static WeaverTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("AssemblyToProcess.dll");
    }

    [Fact]
    public void ValidateHelloWorldIsInjected()
    {
        var type = testResult.Assembly.GetType("TheNamespace.Hello");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.Equal("Hello World", instance.World());
    }
}
```

By default `ExecuteTestRun` will perform a [PeVerify](https://docs.microsoft.com/en-us/dotnet/framework/tools/peverify-exe-peverify-tool) on the resultant assembly


## Icon

<a href="http://thenounproject.com/noun/lego/#icon-No16919" target="_blank">Lego</a> designed by <a href="http://thenounproject.com/timur.zima" target="_blank">Timur Zima</a> from The Noun Project