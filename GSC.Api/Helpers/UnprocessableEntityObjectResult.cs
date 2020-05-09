using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GSC.Api.Helpers
{
    public class GscUnprocessableEntityObjectResult : ObjectResult
    {
        public GscUnprocessableEntityObjectResult(ModelStateDictionary modelState)
            : base(new SerializableError(modelState))
        {
            if (modelState == null) throw new ArgumentNullException(nameof(modelState));
            StatusCode = 422;
        }
    }
}