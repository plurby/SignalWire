using System;
using System.Reflection;

namespace SignalWire.TypeResolvers
{
    public interface ITypeResolver
    {
        Type Resolve(string type);
        Assembly[] GetReferences();
        string[] GetNamespaces();
    }
}