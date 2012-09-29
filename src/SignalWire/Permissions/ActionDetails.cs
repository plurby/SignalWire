using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace SignalWire.Permissions
{
    public class ActionDetails
    {
        public string Collection { get; set; }
        public object Object { get; set; }
        public UserAction Action { get; set; }
        public IPrincipal User { get; set; }
    }
}