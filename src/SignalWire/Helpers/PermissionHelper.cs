using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using SignalWire.Permissions;

namespace SignalWire.Helpers
{
    public class PermissionHelper
    {
         private readonly Type _t;

        internal PermissionHelper(Type t)
        {
            _t = t;
        }

        internal IQueryable OnRead(IPrincipal user, IQueryable collection)
        {
            var perm = _t.GetCustomAttributes
                (typeof(IPermission), true).FirstOrDefault() as IPermission;

            if (perm != null)
                return perm.OnRead(user, collection);
            else
                return collection;
        }

        internal bool HasPermission(ActionDetails a)
        {
            var perm = _t.GetCustomAttributes
                (typeof(IPermission), true).FirstOrDefault() as IPermission;

            if (perm != null)
                return perm.HasPermission(a);
            else
                return true;
        }

    }
}