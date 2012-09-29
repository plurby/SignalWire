using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using SignalWire.Permissions;

namespace SignalWire.Helpers
{
    public class ModelHelper
    {
        private readonly object _model;

        internal ModelHelper(object model)
        {
            _model = model;
        }


        internal List<ValidationResult> Validate(object obj)
        {
            var context = new ValidationContext(obj, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(obj, context, results);
            return results;
        }
    }
}