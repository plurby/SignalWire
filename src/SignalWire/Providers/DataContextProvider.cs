using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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


        protected object Filter(IEnumerable collection, Type type, NameValueCollection param)
        {
            object data = null;

            if (param["$filter"] != null)
            {
                var casted = Cast(collection, type);
                data = Filter(casted, type, param);
            }
            else
            {
                string finalQuery = "";
                if (param["$query"] != null)
                {
                    finalQuery = string.Format("{0} " + param["$query"] + "{1}",
                                               "(from " + type.Name + " row in Rows where ",
                                               " select row)");
                }
                else
                {
                    finalQuery = "(from " + type.Name + " row in Rows select row)";
                }

                if (param["$skip"] != null)
                {
                    int skip;
                    int.TryParse(param["$skip"], out skip);
                    finalQuery += ".Skip(" + skip + ")";
                }
                if (param["$take"] != null)
                {
                    int take;
                    int.TryParse(param["$take"], out take);
                    finalQuery += ".Take(" + take + ")";
                }

                var host = new ScriptingHost { Rows = collection };
                InitializeHost(host);
                host.AddReference(type.Assembly);
                host.ImportNamespace(type.Namespace);
                data = host.Execute(finalQuery);
            }
            return data;
        }



        private object Cast(dynamic src, Type t)
        {
            MethodInfo method = typeof(System.Linq.Enumerable).GetMethod("Cast");
            MethodInfo generic = method.MakeGenericMethod(t);
            return generic.Invoke(this, new object[] { src });
        }

        private object Filter(dynamic src, Type t, NameValueCollection p)
        {
            MethodInfo method = typeof(Linq2Rest.ModelFilterExtensions)
                .GetMethodExt("Filter", typeof(IEnumerable<>), typeof(NameValueCollection) );
            MethodInfo generic = method.MakeGenericMethod(t);
            return generic.Invoke(this, new object[] { src, p });
        }


    }


    public static class GenericExtensions
    {
        /// <summary>
        /// Search for a method by name and parameter types.  Unlike GetMethod(), does 'loose' matching on generic
        /// parameter types, and searches base interfaces.
        /// </summary>
        /// <exception cref="AmbiguousMatchException"/>
        public static MethodInfo GetMethodExt(this Type thisType, string name, params Type[] parameterTypes)
        {
            return GetMethodExt(thisType, name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, parameterTypes);
        }

        /// <summary>
        /// Search for a method by name, parameter types, and binding flags.  Unlike GetMethod(), does 'loose' matching on generic
        /// parameter types, and searches base interfaces.
        /// </summary>
        /// <exception cref="AmbiguousMatchException"/>
        public static MethodInfo GetMethodExt(this Type thisType, string name, BindingFlags bindingFlags, params Type[] parameterTypes)
        {
            MethodInfo matchingMethod = null;

            // Check all methods with the specified name, including in base classes
            GetMethodExt(ref matchingMethod, thisType, name, bindingFlags, parameterTypes);

            // If we're searching an interface, we have to manually search base interfaces
            if (matchingMethod == null && thisType.IsInterface)
            {
                foreach (Type interfaceType in thisType.GetInterfaces())
                    GetMethodExt(ref matchingMethod, interfaceType, name, bindingFlags, parameterTypes);
            }

            return matchingMethod;
        }

        private static void GetMethodExt(ref MethodInfo matchingMethod, Type type, string name, BindingFlags bindingFlags, params Type[] parameterTypes)
        {
            // Check all methods with the specified name, including in base classes
            foreach (MethodInfo methodInfo in type.GetMember(name, MemberTypes.Method, bindingFlags))
            {
                // Check that the parameter counts and types match, with 'loose' matching on generic parameters
                ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                if (parameterInfos.Length == parameterTypes.Length)
                {
                    int i = 0;
                    for (; i < parameterInfos.Length; ++i)
                    {
                        if (!parameterInfos[i].ParameterType.IsSimilarType(parameterTypes[i]))
                            break;
                    }
                    if (i == parameterInfos.Length)
                    {
                        if (matchingMethod == null)
                            matchingMethod = methodInfo;
                        else
                            throw new AmbiguousMatchException("More than one matching method found!");
                    }
                }
            }
        }

        /// <summary>
        /// Special type used to match any generic parameter type in GetMethodExt().
        /// </summary>
        public class T
        { }

        /// <summary>
        /// Determines if the two types are either identical, or are both generic parameters or generic types
        /// with generic parameters in the same locations (generic parameters match any other generic paramter,
        /// but NOT concrete types).
        /// </summary>
        private static bool IsSimilarType(this Type thisType, Type type)
        {
            // Ignore any 'ref' types
            if (thisType.IsByRef)
                thisType = thisType.GetElementType();
            if (type.IsByRef)
                type = type.GetElementType();

            // Handle array types
            if (thisType.IsArray && type.IsArray)
                return thisType.GetElementType().IsSimilarType(type.GetElementType());

            // If the types are identical, or they're both generic parameters or the special 'T' type, treat as a match
            if (thisType == type || ((thisType.IsGenericParameter || thisType == typeof(T)) && (type.IsGenericParameter || type == typeof(T))))
                return true;

            // Handle any generic arguments
            if (thisType.IsGenericType && type.IsGenericType)
            {
                Type[] thisArguments = thisType.GetGenericArguments();
                Type[] arguments = type.GetGenericArguments();
                if (thisArguments.Length == arguments.Length)
                {
                    for (int i = 0; i < thisArguments.Length; ++i)
                    {
                        if (!thisArguments[i].IsSimilarType(arguments[i]))
                            return false;
                    }
                    return true;
                }
            }

            return false;
        }
    }

}