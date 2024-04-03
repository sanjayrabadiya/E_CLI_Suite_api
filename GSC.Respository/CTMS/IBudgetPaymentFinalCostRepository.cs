using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IBudgetPaymentFinalCostRepository : IGenericRepository<BudgetPaymentFinalCost>
    {
        List<BudgetPaymentFinalCostGridDto> GetBudgetPaymentFinalCostList(int projectId, bool isdelete);
        BudgetPaymentFinalCostDto GetFinalBudgetCost(int projectId);
        string Duplicate(BudgetPaymentFinalCostDto budgetPaymentFinalCostDto);
    }
}