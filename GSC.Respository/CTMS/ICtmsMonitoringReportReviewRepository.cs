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
        void SendMailForApproved(CtmsMonitoringReportReview ReviewDto);
        void SendMailToReviewer(CtmsMonitoringReportReviewDto ReviewDto);
        bool GetReview(int CtmsMonitoringReportId);
        void SaveTemplateReview(List<CtmsMonitoringReportReviewDto> ctmsMonitoringReportReviewDtos);
        List<CtmsMonitoringReportReviewHistory> GetCtmsMonitoringReportReviewHistory(int id);
        CtmsMonitoringReportReviewDto GetCtmsMonitoringReportReview(int id);
        bool isAnyReportReviewer(int id);
        bool GetReviewSendToAnyone(int CtmsMonitoringReportId);
        List<DashboardDto> GetSendTemplateList(int ProjectId, int? siteId);
        List<DashboardDto> GetSendBackTemplateList(int ProjectId, int? siteId);
    }
}