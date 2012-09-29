using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SignalWire.Demo.Model
{

    //Simple demo db context   
    public class TaskDb : DbContext
    {
        public DbSet<Task> Tasks { get; set; }
    }
}