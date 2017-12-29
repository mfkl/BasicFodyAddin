[![Chat on Gitter](https://img.shields.io/gitter/room/fody/fody.svg?style=flat&max-age=86400)](https://gitter.im/Fody/Fody)
[![NuGet Status](http://img.shields.io/nuget/v/BasicFodyAddin.Fody.svg?style=flat&max-age=86400)](https://www.nuget.org/packages/BasicFodyAddin.Fody/)

![Icon](https://raw.github.com/Fody/BasicFodyAddin/master/package_icon.png)

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

A project that contains any classes used for compile time metadata. Generally any usage and reference to this is removed at compile time so it is not needed as part of application deployment.


### BasicFodyAddin.Fody Project

The project that does the weaving.


#### Output of the project

It outputs a file named BasicFodyAddin.Fody. The '.Fody' suffix is necessary for it to be picked up by Fody.


#### ModuleWeaver

ModuleWeaver.cs is where the target assembly is modified. Fody will pick up this type during a its processing.

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


#### Nuget construction

Fody addins are deployed as [NuGet](http://nuget.org/) packages. The BasicFodyAddin.Fody builds the package for BasicFodyAddin as part of a build. The output of this project is placed in *SolutionDir*/nugets.

This project uses [pepita](https://github.com/SimonCropp/Pepita) to construct the package but you could also any other mechanism for constructing a NuGet package.

For more information on the NuGet structure of Fody addins see [DeployingAddinsAsNugets](https://github.com/Fody/Fody/wiki/DeployingAddinsAsNugets)


### AssemblyToProcess Project

A target assembly to process and then validate with unit tests.


### Tests Project

This is where you would place your unit tests.

The test assembly contains three parts.


#### 1. WeaverHelper

A helper class that takes the output of AssemblyToProcess and uses ModuleWeaver to process it. It also create a copy of the target assembly suffixed with '2' so a side-by-side comparison of the before and after IL can be done using a decompiler.


#### 2. Verifier

A helper class that runs [peverfiy](http://msdn.microsoft.com/en-us/library/62bwd2yd.aspx) to validate the resultant assembly.


#### 3. Tests

The actual unit tests that use WeaverHelper and Verifier. It has one test to construct and execute the injected class.


### No reference to Fody

Note that there is no reference to Fody nor are any Fody files included in the solution. Interaction with Fody is done by convention at compile time.


## Icon

<a href="http://thenounproject.com/noun/lego/#icon-No16919" target="_blank">Lego</a> designed by <a href="http://thenounproject.com/timur.zima" target="_blank">Timur Zima</a> from The Noun Project