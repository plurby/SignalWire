using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using SignalWire.Helpers;

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
            Type t = ResolveModelType(table);

            NameValueCollection param = query.ToNameValueCollection();

            string finalQuery = "";
            if (param["query"] != null)
            {
                finalQuery = string.Format("{0} " + param["query"] + "{1}",
                                           "(from " + t.Name + " row in Rows where ",
                                           " select row)");
            }
            else
            {
                finalQuery = "(from " + t.Name + " row in Rows select row)";
            }

            if (param["skip"] != null)
            {
                int skip;
                int.TryParse(param["skip"], out skip);
                finalQuery += ".Skip(" + skip + ")";
            }
            if (param["take"] != null)
            {
                int take;
                int.TryParse(param["take"], out take);
                finalQuery += ".Take(" + take + ")";
            }

            MongoDatabase database = GetDatabase();
            MongoCollection collection = database.GetCollection(ResolveModelType(table), table);

            var host = new ScriptingHost {Rows = collection.FindAllAs(t)};
            host.AddReference(t.Assembly);
            host.AddReference(typeof (MongoServer).Assembly);
            host.AddReference(typeof (BsonArray).Assembly);
            host.ImportNamespace(t.Namespace);
            object data = host.Execute(finalQuery);

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