using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.Master
{
    public interface IPassthroughMilestoneRepository : IGenericRepository<PassthroughMilestone>
    {
        IList<PassthroughMilestoneGridDto> GetPassthroughMilestoneList(int parentProjectId, bool isDeleted);
        string DuplicatePaymentMilestone(PassthroughMilestone paymentMilestone);
        decimal GetPassthroughMilestoneAmount(PassthroughMilestoneDto paymentMilestoneDto); 
        void AddPaymentMilestonePassThroughCostDetail(PassthroughMilestoneDto paymentMilestoneDto);
        void DeletePaymentMilestonePassThroughCostDetail(int Id);
        void ActivePaymentMilestonePassThroughCostDetail(int Id);
        List<DropDownDto> GetPassThroughCostActivity(int projectId);
        BudgetPaymentFinalCostDto GetFinalPassthroughTotal(int projectId);

    }
}
