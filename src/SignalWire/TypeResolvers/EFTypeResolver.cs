using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SignalWire.Providers;

namespace SignalWire.TypeResolvers
{
    public class EFTypeResolver : ITypeResolver
    {
        private readonly Type _t;

        public EFTypeResolver(Type t)
        {
            _t = t;
        }

        #region ITypeResolver Members

        public Type Resolve(string type)
        {
            return GetSetType(type);
        }

        public Assembly[] GetReferences()
        {
            return null;
        }

        public string[] GetNamespaces()
        {
            return null;
        }

        #endregion

        private Type GetSetType(string name)
        {
            IEnumerable<PropertyInfo> props = _t.GetProperties().Where
                (p => p.PropertyType.ToGenericTypeString().StartsWith("DbSet<"));
            IEnumerable<Type> t = from p in props
                                  where string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)
                                  let declaringType = p.PropertyType
                                  where declaringType != null
                                  let type = declaringType.GetGenericArguments().First()
                                  select type;
            return t.FirstOrDefault();
        }
    }
}