using System;
using System.Collections.Generic;
using System.Reflection;
using SignalWire.TypeResolvers;

namespace SignalWire.Providers
{
    public abstract class DataContextProvider : IDataContextProvider
    {
        private ITypeResolver _resolver;

        #region IDataContextProvider Members

        public virtual Type ResolveModelType(string table)
        {
            if (_resolver == null) return null;
            return _resolver.Resolve(table);
        }

        public abstract Result Add(string table, object obj);
        public abstract Result Remove(string table, object obj);
        public abstract Result Update(string table, object obj);
        public abstract Result Read(string table, IDictionary<string, string> query);
        public abstract Result Query(string query);
        public abstract string[] GetCollectionNames();
        public abstract ConnectionSettings GetSettings();

        #endregion

        public void SetResolver(ITypeResolver resolver)
        {
            _resolver = resolver;
        }

        internal ITypeResolver GetResolver()
        {
            return _resolver;
        }


        protected void InitializeHost(ScriptingHost host)
        {
            ITypeResolver resolver = GetResolver();
            Assembly[] rs = resolver.GetReferences();
            string[] ns = resolver.GetNamespaces();

            if (rs != null)
                foreach (Assembly r in rs)
                    host.AddReference(r);

            if (ns != null)
                foreach (string n in ns)
                    host.ImportNamespace(n);
        }
    }
}