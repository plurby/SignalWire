using System.Linq;
using System.Security.Principal;

namespace SignalWire.Permissions
{
    public interface IPermission
    {
        bool HasPermission (ActionDetails a);
        IQueryable OnRead(IPrincipal user, IQueryable rows);
    }
}