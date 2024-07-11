using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.CTMS
{
    public interface ICtmsMonitoringReportRepository : IGenericRepository<CtmsMonitoringReport>
    {
        CtmsMonitoringReportFormDto GetCtmsMonitoringReportVariableValue(CtmsMonitoringReportFormDto designTemplateDto, int ctmsMonitoringReportId);
        CtmsMonitoringReportBasic GetFormBasic(int ManageMonitoringReportId);
        string GetMonitoringFormApprovedOrNot(int projectId, int siteId, int tabNumber);
    }
}