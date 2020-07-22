
using GSC.Helper.Validation;
using Microsoft.AspNetCore.Mvc.Filters;


namespace GSC.Api.Helpers
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid) context.Result = new UnprocessableEntityObjectResult(context.ModelState);
        }
    }
}