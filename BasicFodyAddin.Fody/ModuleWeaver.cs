using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using Fody;

public class ModuleWeaver: BaseModuleWeaver
{
    TypeSystem typeSystem;

    public override void Execute()
    {
        typeSystem = ModuleDefinition.TypeSystem;
        var ns = GetNamespace();
        var newType = new TypeDefinition(ns, "Hello", TypeAttributes.Public, typeSystem.Object);

        AddConstructor(newType);

        AddHelloWorld(newType);

        ModuleDefinition.Types.Add(newType);
        LogInfo("Added type 'Hello' with method 'World'.");
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        return Enumerable.Empty<string>();
    }

    string GetNamespace()
    {
        var attributes = ModuleDefinition.Assembly.CustomAttributes;
        var namespaceAttribute = attributes.FirstOrDefault(x => x.AttributeType.FullName == "NamespaceAttribute");
        if (namespaceAttribute == null)
        {
            return null;
        }
        attributes.Remove(namespaceAttribute);
        return (string) namespaceAttribute.ConstructorArguments.First().Value;
    }

    void AddConstructor(TypeDefinition newType)
    {
        var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, typeSystem.Void);
        var objectConstructor = ModuleDefinition.ImportReference(typeSystem.Object.Resolve().GetConstructors().First());
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Call, objectConstructor);
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
    }

    void AddHelloWorld(TypeDefinition newType)
    {
        var method = new MethodDefinition("World", MethodAttributes.Public, typeSystem.String);
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldstr, "Hello World");
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
    }

    public override bool ShouldCleanReference => true;
}