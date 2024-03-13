using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Report;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GSC.Report
{
    public interface IReportSyncfusion
    {
        FileStreamResult GetProjectDesign(ReportSettingNew reportSetting);
        Task<string> DossierPdfReportGenerate(ReportSettingNew reportSetting, JobMonitoring jobMonitoring);
        Task<string> ScreeningPdfReportGenerate(ScreeningReportSetting reportSetting, JobMonitoring jobMonitoring);
    }
}
