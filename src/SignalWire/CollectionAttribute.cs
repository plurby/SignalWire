using System;

namespace SignalWire
{
    public class CollectionAttribute : Attribute
    {
        public CollectionAttribute(string name)
        {
            CollectionName = name;
        }

        public string CollectionName { get; set; }
    }
}