using System;
using System.Collections.Generic;

namespace SignalWire.Providers
{
    public interface IDataContextProvider
    {
        Result Add(string table, object obj);
        Result Remove(string table, object obj);
        Result Update(string table, object obj);
        Result Read(string table, IDictionary<string, string> query);
        Result Query(string query);
        Type ResolveModelType(string table);
        string[] GetCollectionNames();
        ConnectionSettings GetSettings();
    }
}