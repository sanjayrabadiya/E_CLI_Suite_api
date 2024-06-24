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
        List<decimal> GetEstimatedMilestoneAmount(int ParentProjectId,int visitId);
        decimal GetFinalPatienTotal(int projectId);
        List<DropDownDto> GetVisitDropDown(int parentProjectId);
        string UpdatePaybalAmount(PatientMilestone paymentMilestone);
    }
}
