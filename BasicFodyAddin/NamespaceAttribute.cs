using System;

[AttributeUsage(AttributeTargets.Assembly)]
public class NamespaceAttribute:Attribute
{
    public NamespaceAttribute(string @namespace)
    {

    }
}