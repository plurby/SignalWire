using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SignalWire
{
    public class Result
    {
        public bool Success { get; set; }
        public List<ValidationResult> ValidationResults { get; set; }
        public string Error { get; set; }
        public object Data { get; set; }
    }
}