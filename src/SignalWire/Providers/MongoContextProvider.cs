using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using SignalWire.Helpers;
using Linq2Rest;

namespace SignalWire.Providers
{
    public class MongoContextProvider : DataContextProvider
    {
        public override Result Add(string table, object obj)
        {
            MongoDatabase database = GetDatabase();
            MongoCollection collection = database.GetCollection(ResolveModelType(table), table);
            collection.Insert(obj);
            return new Result {Data = obj, Success = true};
        }

        public override Result Remove(string table, object obj)
        {
            MongoDatabase database = GetDatabase();
            MongoCollection collection = database.GetCollection(ResolveModelType(table), table);
            dynamic query = MongoDB.Driver.Builders.Query.EQ("_id", ((dynamic) obj).Id);
            collection.Remove(query);
            return new Result {Data = obj, Success = true};
        }

        public override Result Update(string table, object obj)
        {
            MongoDatabase database = GetDatabase();
            MongoCollection collection = database.GetCollection(ResolveModelType(table), table);
            dynamic query = MongoDB.Driver.Builders.Query.EQ("_id", ((dynamic) obj).Id);
            collection.Save(ResolveModelType(table), obj);
            return new Result {Data = obj, Success = true};
        }

        public override Result Read(string table, IDictionary<string, string> query)
        {
            var type = ResolveModelType(table);
            var param = query.ToNameValueCollection();
            var database = GetDatabase();
            var collection = database.GetCollection(ResolveModelType(table), table);
            var result = collection.FindAllAs(type);
            var data = Filter(result, type, param);
         
            return new Result
                {
                    Success = true,
                    Data = data
                };
        }

        public override Result Query(string query)
        {
            MongoDatabase database = GetDatabase();
            var host = new ScriptingHost(database) {};
            InitializeHost(host);
            object data = host.Execute(query);
            return new Result
                {
                    Success = true,
                    Data = data
                };
        }


        public override string[] GetCollectionNames()
        {
            MongoDatabase db = GetDatabase();
            return db.GetCollectionNames().ToArray();
        }

        private MongoDatabase GetDatabase()
        {
            // Create server settings to pass connection string, timeout, etc.
            ConnectionSettings settings = GetSettings();
            MongoServer server = null;
            if (string.IsNullOrEmpty(settings.ConnectionString))
            {
                var ms = new MongoServerSettings
                    {
                        Server = new MongoServerAddress(settings.Server,
                                                        int.Parse(settings.Port))
                    };
                // Create server object to communicate with our server
                server = new MongoServer(ms);
            }
            else
            {
                server = MongoServer.Create(settings.ConnectionString);
            }
            // Get our database instance to reach collections and data
            return server.GetDatabase(settings.Database);
        }

        public override ConnectionSettings GetSettings()
        {
            return new ConnectionSettings {Server = "localhost", Database = "data", Port = "27017"};
        }
    }
}