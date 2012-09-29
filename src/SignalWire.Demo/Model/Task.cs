using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SignalWire.Demo.Model
{
    //Simple POCO class to represent a task
    [Collection("tasks")]
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

        public bool Completed { get; set; }
    }


}