using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Trustly.Api.Client.Validation
{
    public class DataAnnotationsValidator
    {
        private static readonly ICollection<ValidationResult> EMPTY_VALIDATION_RESULTS = new List<ValidationResult>().AsReadOnly();

        public ICollection<ValidationResult> TryValidateObjectRecursive<T>(T obj)
        {
            return TryValidateObjectRecursive(obj, new HashSet<object>());
        }

        private ICollection<ValidationResult> TryValidateObjectRecursive<T>(T obj, ISet<object> validatedObjects)
        {
            // We never validate null values, or the same value more than once.
            if (obj == null || validatedObjects.Contains(obj))
            {
                return EMPTY_VALIDATION_RESULTS;
            }

            validatedObjects.Add(obj);
            var list = new List<ValidationResult>();
            var result = Validator.TryValidateObject(obj, new ValidationContext(obj, null), list, true);

            foreach (var property in obj.GetType().GetProperties().Where(this.IsSupportedProperty))
            {
                if (property.PropertyType == typeof(string) || property.PropertyType.IsValueType) continue;

                var value = property.GetValue(obj, null);
                if (value is IEnumerable asEnumerable)
                {
                    foreach (var v in asEnumerable)
                    {
                        list.AddRange(this.WrapValidationResults(v, property, validatedObjects));
                    }
                }
                else
                {
                    list.AddRange(this.WrapValidationResults(value, property, validatedObjects));
                }
            }

            return list;
        }

        private bool IsSupportedProperty(PropertyInfo prop)
        {
            return prop.CanRead && prop.GetIndexParameters().Length == 0;
        }

        private IEnumerable<ValidationResult> WrapValidationResults(object entry, PropertyInfo property, ISet<object> validatedObjects)
        {
            return this.TryValidateObjectRecursive(entry, validatedObjects)
                        .Select(vr => new ValidationResult(vr.ErrorMessage, vr.MemberNames.Select(x => property.Name + '.' + x)));
        }
    }
}
