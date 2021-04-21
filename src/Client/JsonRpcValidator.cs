using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Trustly.Api.Client.Validation;
using Trustly.Api.Domain.Exceptions;

namespace Trustly.Api.Client
{
    public class JsonRpcValidator
    {
        public void Validate(object jsonRpcRequest)
        {
            var validator = new DataAnnotationsValidator();
            var results = new List<ValidationResult>();

            bool isValid = validator.TryValidateObjectRecursive(jsonRpcRequest, results);

            if (!isValid)
            {
                throw new TrustlyDataException(string.Join(", ", results.Select(r => r.ErrorMessage)));
            }
        }
    }
}
