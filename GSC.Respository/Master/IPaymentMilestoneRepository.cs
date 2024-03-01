using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.Master
{
    public interface IPaymentMilestoneRepository : IGenericRepository<PaymentMilestone>
    {
        IList<PaymentMilestoneGridDto> GetPaymentMilestoneList(int parentProjectId, int? siteId, int? countryId, bool isDeleted);
        string DuplicatePaymentMilestone(PaymentMilestone paymentMilestone);
        List<DropDownDto> GetTaskListforMilestone(int parentProjectId, int? siteId, int? countryId);
        decimal GetEstimatedMilestoneAmount(PaymentMilestoneDto paymentMilestoneDto);
        void AddPaymentMilestoneTaskDetail(PaymentMilestoneDto paymentMilestoneDto);
        void DeletePaymentMilestoneTaskDetail(int Id);
        void ActivePaymentMilestoneTaskDetail(int Id);
    }
}
