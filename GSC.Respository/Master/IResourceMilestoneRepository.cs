using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public interface IResourceMilestoneRepository : IGenericRepository<ResourceMilestone>
    {
        IList<ResourceMilestoneGridDto> GetPaymentMilestoneList(int parentProjectId, int? siteId, int? countryId, bool isDeleted);
        string DuplicatePaymentMilestone(ResourceMilestone paymentMilestone);
        List<DropDownTaskListforMilestoneDto> GetTaskListforMilestone(int parentProjectId, int? siteId, int? countryId);
        void DeletePaymentMilestoneTaskDetail(int Id);
        void ActivePaymentMilestoneTaskDetail(int Id);
        decimal GetFinalResourceTotal(int projectId);
        Task SendDueResourceMilestoneEmail();
        IList<ResourceMilestoneGridDto> GetTaskPaymentDueList(int parentProjectId, int? siteId, int? countryId, bool isDeleted, CTMSPaymentDue cTMSPaymentDue);
        IList<ResourceMilestoneGridDto> GetTaskPaymentBudgetList();
    }
}
