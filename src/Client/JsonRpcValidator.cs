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
            var results = validator.TryValidateObjectRecursive(jsonRpcRequest);

            if (results.Count > 0)
            {
                throw new TrustlyDataException(string.Join(", ", results.Select(r => r.ErrorMessage)));
            }
        }
    }
}
