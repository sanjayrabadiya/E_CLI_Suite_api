using GSC.Data.Dto.Common;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Custom;
using GSC.Data.Dto.Report.Pdf;
using GSC.Data.Entities.Custom;
using GSC.Data.Entities.Report;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Telerik.Reporting;

namespace GSC.Report.Common
{
    public interface IReportBaseRepository
    {
        FileStreamResult ReportRun(string reportPath, SqlDataSource sqlDataSource);
        SqlDataSource DataSource(string strSql, SqlDataSourceParameterCollection parameter);
        FileStreamResult ReportRunNew(string reportPath, SqlDataSource sqlDataSource, ReportSettingNew reportSetiingNew, CompanyDataDto companyData, FileSaveInfo fileInfo);
        string CompleteJobMonitoring(JobMonitoring jobMonitoring);
        void WriteLog(string log, string path, JobMonitoring jobMonitoring);
        List<DossierReportDto> GetBlankPdfData(ReportSettingNew reportSetting);
        List<DossierReportDto> GetDataPdfReport(ReportSettingNew reportSetting);
    }
}