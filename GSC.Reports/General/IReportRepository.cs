using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;

namespace GSC.Reports.General
{
    public interface IReportRepository
    {
        FileStreamResult ReportGenerate(string reportPath, SqlDataSource sqlDataSource);
        SqlDataSource GetSqlDataSource(string storedProcedure, SqlDataSourceParameterCollection parameter);
    }
}
