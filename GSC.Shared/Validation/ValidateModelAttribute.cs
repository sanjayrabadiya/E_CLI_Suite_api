using Microsoft.AspNetCore.Mvc.Filters;


namespace GSC.Shared.Validation
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid) context.Result = new UnprocessableEntityObjectResult(context.ModelState);
        }
    }
}