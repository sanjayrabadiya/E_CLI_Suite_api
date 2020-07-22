using System.Linq;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GSC.Api.Helpers
{
    public class TransactionFilter : ActionFilterAttribute
    {
        private readonly IUnitOfWork _uow;

        public TransactionFilter(IUnitOfWork  uow)
        {
           _uow = uow;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
           
            if (context.Filters.Any(t => t.GetType() == typeof(TransactionRequiredAttribute)))
            {
                if (context.Exception == null && context.ModelState.IsValid)
                    _uow.Commit();
                else
                    _uow.Rollback();
            }

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.Filters.Any(t => t.GetType() == typeof(TransactionRequiredAttribute)))
                return;
            _uow.Begin();
        }

     
    }

    public class TransactionRequiredAttribute : ActionFilterAttribute
    {
    }
}