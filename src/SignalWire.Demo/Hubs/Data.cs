using SignalWire.Demo.Model;
using SignalWire.Permissions;
using SignalWire.Providers;

namespace SignalWire.Demo.Hubs
{
    public class Data : DataHub<EFContextProvider<TaskDb>>
    {
    }

}