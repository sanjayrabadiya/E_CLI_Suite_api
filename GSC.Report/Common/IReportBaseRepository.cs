using GSC.Data.Dto.Configuration;
using Microsoft.AspNetCore.Mvc;
using Telerik.Reporting;

namespace GSC.Report.Common
{
    public interface IReportBaseRepository
    {
        FileStreamResult ReportRun(string reportPath, SqlDataSource sqlDataSource);
        SqlDataSource DataSource(string strSql, SqlDataSourceParameterCollection parameter);
        FileStreamResult ReportRunNew(string reportPath, SqlDataSource sqlDataSource, ReportSettingNew reportSetiingNew, CompanyData companyData);
    }
}