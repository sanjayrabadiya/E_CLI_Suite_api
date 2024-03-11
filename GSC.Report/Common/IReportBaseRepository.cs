using GSC.Data.Dto.Common;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Custom;
using GSC.Data.Dto.Report.Pdf;
using GSC.Data.Entities.Custom;
using GSC.Data.Entities.Report;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
    }
}