using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.Master
{
    public interface IPassthroughMilestoneRepository : IGenericRepository<PassthroughMilestone>
    {
        IList<PassthroughMilestoneGridDto> GetPaymentMilestoneList(int parentProjectId, int? siteId, int? countryId, bool isDeleted);
        string DuplicatePaymentMilestone(PassthroughMilestone paymentMilestone);
        List<DropDownDto> GetTaskListforMilestone(int parentProjectId, int? siteId, int? countryId);
        decimal GetEstimatedMilestoneAmount(PassthroughMilestoneDto paymentMilestoneDto);
        void AddPaymentMilestoneTaskDetail(PassthroughMilestoneDto paymentMilestoneDto);
        void DeletePaymentMilestoneTaskDetail(int Id);
        void ActivePaymentMilestoneTaskDetail(int Id);
        void AddPaymentMilestoneVisitDetail(PassthroughMilestoneDto paymentMilestoneDto);
        void DeletePaymentMilestoneVisitDetail(int Id);
        void ActivePaymentMilestoneVisitDetail(int Id);
        void AddPaymentMilestonePassThroughCostDetail(PassthroughMilestoneDto paymentMilestoneDto);
        void DeletePaymentMilestonePassThroughCostDetail(int Id);
        void ActivePaymentMilestonePassThroughCostDetail(int Id);
        List<DropDownProcedureDto> GetParentProjectDropDown( int parentProjectId);
        List<DropDownDto> GetVisitDropDown(int parentProjectId, int procedureId);
        List<DropDownDto> GetPassThroughCostActivity(int projectId);
       
    }
}
