using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Report.Pdf;
using GSC.Data.Entities.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GSC.Report.Common
{
    public interface IReportBaseRepository
    {
         string CompleteJobMonitoring(JobMonitoring jobMonitoring);
        void WriteLog(string log1, string path, JobMonitoring jobMonitoring);
        List<DossierReportDto> GetBlankPdfData(ReportSettingNew reportSetting);
        List<DossierReportDto> GetDataPdfReport(ReportSettingNew reportSetting);
        List<ScreeningPdfReportDto> GetScreeningDataPdfReport(ScreeningReportSetting reportSetting);
        List<ScreeningPdfReportDto> GetScreeningBlankPdfData(ScreeningReportSetting reportSetting);
        Task<List<DossierReportDto>> GetBlankPdfDataAsync(ReportSettingNew reportSetting);

        Task<List<DossierReportDto>> GetDataPdfReportAsync(ReportSettingNew reportSetting);
    }
}