using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.Master
{
    public interface IResourceMilestoneRepository : IGenericRepository<ResourceMilestone>
    {
        IList<ResourceMilestoneGridDto> GetPaymentMilestoneList(int parentProjectId, int? siteId, int? countryId, bool isDeleted);
        string DuplicatePaymentMilestone(ResourceMilestone paymentMilestone);
        List<DropDownDto> GetTaskListforMilestone(int parentProjectId, int? siteId, int? countryId);
        decimal GetEstimatedMilestoneAmount(ResourceMilestoneDto paymentMilestoneDto);
        void AddPaymentMilestoneTaskDetail(ResourceMilestoneDto paymentMilestoneDto);
        void DeletePaymentMilestoneTaskDetail(int Id);
        void ActivePaymentMilestoneTaskDetail(int Id);

        List<DropDownProcedureDto> GetParentProjectDropDown( int parentProjectId);
        List<DropDownDto> GetVisitDropDown(int parentProjectId, int procedureId);
        List<DropDownDto> GetPassThroughCostActivity(int projectId);
       
    }
}
