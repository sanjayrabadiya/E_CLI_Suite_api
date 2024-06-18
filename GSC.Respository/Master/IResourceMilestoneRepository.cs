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
        IList<ResourceMilestoneGridDto> GetPaymentMilestoneList(bool isDeleted, int studyId, int siteId, int countryId, CtmsStudyTaskFilter filterType);
        string DuplicatePaymentMilestone(ResourceMilestone paymentMilestone);
        List<DropDownTaskListforMilestoneDto> GetTaskListforMilestone(int studyId, int siteId, int countryId, CtmsStudyTaskFilter filterType);
        void DeletePaymentMilestoneTaskDetail(int Id);
        void ActivePaymentMilestoneTaskDetail(int Id);
        decimal GetFinalResourceTotal(int projectId);
        Task SendDueResourceMilestoneEmail();
        IList<ResourceMilestoneGridDto> GetTaskPaymentDueList(int parentProjectId, int? siteId, int? countryId, bool isDeleted, CTMSPaymentDue cTMSPaymentDue);
        IList<ResourceMilestoneGridDto> GetTaskPaymentBudgetList();
        string UpdatePaybalAmount(ResourceMilestone paymentMilestone);
    }
}
