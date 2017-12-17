using System;
using System.IO;
using System.Reflection;
using Fody;
using Mono.Cecil;
using Xunit;
#pragma warning disable 618

public class WeaverTests:IDisposable
{
    Assembly assembly;
    string newAssemblyPath;
    string assemblyPath;

    public WeaverTests()
    {
        assemblyPath = Path.Combine(CodeBaseLocation.CurrentDirectory, "AssemblyToProcess.dll");

        newAssemblyPath = assemblyPath.Replace(".dll", "2.dll");
        File.Copy(assemblyPath, newAssemblyPath, true);

        using (var moduleDefinition = ModuleDefinition.ReadModule(assemblyPath))
        {
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition
            };

            weavingTask.Execute();
            moduleDefinition.Write(newAssemblyPath);
        }

        assembly = Assembly.Load(File.ReadAllBytes(newAssemblyPath));
    }

    [Fact]
    public void ValidateHelloWorldIsInjected()
    {
        var type = assembly.GetType("TheNamespace.Hello");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.Equal("Hello World", instance.World());
    }

    [Fact]
    public void PeVerify()
    {
        PeVerifier.ThrowIfDifferent(assemblyPath, newAssemblyPath, new []{ "0x80070002" });
    }

    public void Dispose()
    {
       File.Delete(newAssemblyPath);
    }
}