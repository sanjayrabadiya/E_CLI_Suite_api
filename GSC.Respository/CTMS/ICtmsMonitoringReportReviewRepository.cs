using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsMonitoringReportReviewRepository : IGenericRepository<CtmsMonitoringReportReview>
    {
        List<CtmsMonitoringReportReviewDto> UserRoles(int Id, int ProjectId);
        void SendMailForApproved(CtmsMonitoringReportReviewDto ReviewDto);
        void SendMailToReviewer(CtmsMonitoringReportReviewDto ReviewDto);
        bool GetReview(int CtmsMonitoringReportId);
    }
}