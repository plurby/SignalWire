using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace SignalWire.Permissions
{
    public class DefaultPermission : IPermission
    {
        public virtual bool HasPermission(ActionDetails a)
        {
            return true;
        }

        public virtual IQueryable OnRead(IPrincipal user, IQueryable rows)
        {
            return rows;
        }
    }
}