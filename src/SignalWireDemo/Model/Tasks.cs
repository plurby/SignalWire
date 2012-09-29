using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SignalWireDemo.Model
{

    //Simple POCO class to represent a task
    public class Task
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "Subject cannot be longer than 40 characters.")]
        public string Subject { get; set; }
        [Required]
        [MaxLength(200, ErrorMessage = "Details cannot be longer than 40 characters.")]
        public string Details { get; set; }
        [Required]
        public string Owner { get; set; }
    }


    //Simple demo db context   
    public class DemoDb : DbContext
    {
        public DbSet<Task> Tasks { get; set; }
    }
}