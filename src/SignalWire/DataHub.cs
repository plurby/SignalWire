using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalR.Hubs;
using SignalWire.Helpers;
using SignalWire.Permissions;
using SignalWire.Providers;
using SignalWire.TypeResolvers;

namespace SignalWire
{
    public abstract class DataHub<TContext> : Hub
        where TContext : DataContextProvider, new()
    {
        private readonly TContext _context = new TContext();

        protected DataHub()
        {
       
            var resolver = GetTypeResolver();
            if (resolver != null)
                _context.SetResolver(resolver);

            if (_context.GetResolver() == null)
            {
                var asmr = new AssemblyTypeResolver();
                var asm = Assembly.GetCallingAssembly();
                var namesp = asm.GetExportedTypes()
                    .FirstOrDefault(t => t.Namespace != null && t.Namespace.Contains(".Model"));
                asmr.AddReference(asm);
                if (namesp != null) asmr.AddNamespace(namesp.Namespace);
                _context.SetResolver(asmr);
            }
        }


        public Result Add(string collection, JObject jobj)
        {
            Result result = RequestCheck(collection, UserAction.Add, jobj);
            if (!result.Success) return result;

            try
            {
                object obj = result.Data;
                return _context.Add(collection, obj);
            }
            catch (Exception ex)
            {
                return new Result
                    {
                        Success = false,
                        Error = ex.Message
                    };
            }
        }

        public Result Remove(string collection, JObject jobj)
        {
            Result result = RequestCheck(collection, UserAction.Remove, jobj);
            if (!result.Success) return result;

            try
            {
                object obj = result.Data;
                return _context.Remove(collection, obj);
            }
            catch (Exception ex)
            {
                return new Result
                    {
                        Success = false,
                        Error = ex.Message
                    };
            }
        }

        public Result Update(string collection, JObject jobj)
        {
            Result result = RequestCheck(collection, UserAction.Update, jobj);
            if (!result.Success) return result;

            try
            {
                object obj = result.Data;
                return _context.Update(collection, obj);
            }
            catch (Exception ex)
            {
                return new Result
                    {
                        Success = false,
                        Error = ex.Message
                    };
            }
        }


        public Result Read(string collection, IDictionary<string, string> query)
        {
            var details = new ActionDetails
                {
                    Collection = collection,
                    User = Context.User,
                    Action = UserAction.Read
                };

            if (!HasPermission(details))
            {
                return new Result
                    {
                        Success = false,
                        Error = "No Permission"
                    };
            }

            try
            {
                var q = query ?? new Dictionary<string, string>();
                var result = _context.Read(collection,q );
                return result;
            }
            catch (Exception ex)
            {
                return new Result
                    {
                        Success = false,
                        Error = ex.Message
                    };
            }
        }


        public Result Query(string query)
        {
            var details = new ActionDetails
                {
                    User = Context.User,
                    Action = UserAction.ReadAll
                };

            if (!HasPermission(details))
            {
                return new Result
                    {
                        Success = false,
                        Error = "No Permission"
                    };
            }

            try
            {
                Result result = _context.Query(query);
                return result;
            }
            catch (Exception ex)
            {
                return new Result
                    {
                        Success = false,
                        Error = ex.Message
                    };
            }
        }


        public string[] GetCollections()
        {
            return _context.GetCollectionNames().Select
                (item => item.FirstCharToLower()).ToArray();
        }


        protected virtual bool HasPermission
            (ActionDetails a)
        {
            return true;
        }

        protected virtual ITypeResolver GetTypeResolver()
        {
            return null;
        }

        protected virtual IQueryable OnRead
            (IPrincipal user, IQueryable rows)
        {
            return rows;
        }


        private Result RequestCheck(string collection, UserAction action, JObject jobj)
        {

            var type = _context.ResolveModelType(collection);
            var obj = ToPocoType(jobj, type);
            var validator = new ModelHelper(obj);
            var permission = new PermissionHelper(obj.GetType()); 

            if (type == null || obj==null)
            {
                throw new InvalidOperationException
                    (string.Format("Unable to resolve the type or empty object passed for {0}. Try overloading GetTypeResolver in your Datahub",
                                   collection));
            }
            var details = new ActionDetails
                {
                    Collection = collection,
                    Object = obj,
                    User = Context.User,
                    Action = action
                };

            if (!HasPermission(details) || !permission.HasPermission(details))
            {
                return new Result {Success = false, Error = "No permission"};
            }

            var result = validator.Validate(obj);

            if (result.Count > 0)
            {
                return new Result
                    {
                        Success = false,
                        ValidationResults = result,
                        Error = "Validation Failed"
                    };
            }

            return new Result {Success = true, Data = obj};
        }


        private object ToPocoType(JObject jobj, Type objtype)
        {
            var serializer = new JsonSerializer();
            return serializer.Deserialize(new JTokenReader(jobj), objtype);
        }

        protected object Execute(string code)
        {
            var details = new ActionDetails
                {
                    User = Context.User,
                    Action = UserAction.ReadAll
                };

            if (!HasPermission(details))
            {
                return new Result
                    {
                        Success = false,
                        Error = "No Permission"
                    };
            }

            try
            {
                var host = new ScriptingHost {Context = Caller};
                code = code.Replace("[[", " GetValue(" + "\"");
                code = code.Replace("]]", "\"" + ") ");
                object result = host.Execute(code);
                return new Result {Success = true, Data = result};
            }
            catch (Exception ex)
            {
                return new Result {Success = false, Error = ex.Message};
            }
        }


    }


   
}