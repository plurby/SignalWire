using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using SignalWireDemo.Model;
using SignalWire;

namespace SignalWireDemo.Hubs
{
    public class Data : DataHub<DbContextProvider<DemoDb>>
    {
        public override bool OnAction(ActionDetails a)
        {
            return true;
        }
    }
}

