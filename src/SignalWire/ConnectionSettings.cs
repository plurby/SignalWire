using System.Collections.Generic;

namespace SignalWire
{
    public class ConnectionSettings : Dictionary<string, string>
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
    }
}