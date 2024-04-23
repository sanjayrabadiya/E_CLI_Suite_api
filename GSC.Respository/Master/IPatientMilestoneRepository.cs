using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.Master
{
    public interface IPatientMilestoneRepository : IGenericRepository<PatientMilestone>
    {
        IList<PatientMilestoneGridDto> GetPaymentMilestoneList(int parentProjectId, bool isDeleted);
        string DuplicatePaymentMilestone(PatientMilestone paymentMilestone);
        List<DropDownDto> GetTaskListforMilestone(int parentProjectId, int? siteId, int? countryId);
        decimal GetEstimatedMilestoneAmount(PatientMilestoneDto paymentMilestoneDto);
        void AddPaymentMilestoneVisitDetail(PatientMilestoneDto paymentMilestoneDto);
        void DeletePaymentMilestoneVisitDetail(int Id);
        void ActivePaymentMilestoneVisitDetail(int Id);
        List<DropDownProcedureDto> GetParentProjectDropDown( int parentProjectId);
        List<DropDownDto> GetVisitDropDown(int parentProjectId);
        List<DropDownDto> GetPassThroughCostActivity(int projectId);
       
    }
}
