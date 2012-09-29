using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using SignalWire.Helpers;
using SignalWire.TypeResolvers;

namespace SignalWire.Providers
{
    public class EFContextProvider<TContext> : DataContextProvider
        where TContext : DbContext, new()
    {
        public EFContextProvider()
        {
            var resolver = new EFTypeResolver(typeof (TContext));
            SetResolver(resolver);
        }

        public override Result Add(string table, object obj)
        {
            var db = new TContext();
            db.Set(ResolveModelType(table)).Add(obj);
            db.SaveChanges();

            return new Result
                {
                    Success = true,
                    Data = obj
                };
        }

        public override Result Remove(string table, object obj)
        {
            var db = new TContext();
            db.Set(ResolveModelType(table)).Attach(obj);
            db.Set(ResolveModelType(table)).Remove(obj);
            db.SaveChanges();
            return new Result {Success = true};
        }

        public override Result Update(string table, object obj)
        {
            var db = new TContext();
            db.Set(ResolveModelType(table)).Attach(obj);
            db.SaveChanges();
            return new Result {Success = true};
        }


        public override Result Read(string table, IDictionary<string, string> query)
        {
            var db = new TContext();
            NameValueCollection param = query.ToNameValueCollection();
            Type type = ResolveModelType(table);
            DbSet set = db.Set(type);
            string finalQuery = "";
            if (param["query"] != null)
            {
                finalQuery = string.Format("{0} " + param["query"] + "{1}",
                                           "(from " + type.Name + " row in Rows where ",
                                           " select row)");
            }
            else
            {
                finalQuery = "(from " + type.Name + " row in Rows select row)";
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


            var host = new ScriptingHost {Rows = set};
            host.AddReference(typeof (TContext).Assembly);
            host.ImportNamespace(typeof (TContext).Namespace);
            object data = host.Execute(finalQuery);

            return new Result
                {
                    Success = true,
                    Data = data
                };
        }


        public override Result Query(string query)
        {
            var db = new TContext();

            var host = new ScriptingHost(db) {};
            host.AddReference(typeof (TContext).Assembly);
            host.ImportNamespace(typeof (TContext).Namespace);
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
            IEnumerable<PropertyInfo> props = typeof (TContext).GetProperties().Where
                (p => p.PropertyType.ToGenericTypeString().StartsWith("DbSet<"));

            IEnumerable<string> propNames = from p in props
                                            select p.Name;
            return propNames.ToArray();
        }

        public override ConnectionSettings GetSettings()
        {
            return new ConnectionSettings();
        }
    }


    public static class GenericTypeExtensions
    {
        public static string ToGenericTypeString(this Type t)
        {
            if (!t.IsGenericType)
                return t.Name;
            string genericTypeName = t.GetGenericTypeDefinition().Name;
            genericTypeName = genericTypeName.Substring(0,
                                                        genericTypeName.IndexOf('`'));
            string genericArgs = string.Join(",",
                                             t.GetGenericArguments()
                                                 .Select(ToGenericTypeString).ToArray());
            return genericTypeName + "<" + genericArgs + ">";
        }
    }
}