using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.CTMS;


namespace GSC.Respository.CTMS
{
    public interface ICtmsMonitoringReportRepository : IGenericRepository<CtmsMonitoringReport>
    {
        CtmsMonitoringReportFormDto GetCtmsMonitoringReportVariableValue(CtmsMonitoringReportFormDto designTemplateDto, int CtmsMonitoringReportId);
        CtmsMonitoringReportBasic GetFormBasic(int ManageMonitoringReportId);
        string GetMonitoringFormApprovedOrNOt(int projectId, int siteId, int tabNumber);
    }
}