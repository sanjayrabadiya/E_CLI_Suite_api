using System;
using System.Collections;
using System.Data;
using System.IO;
using GSC.Reports.General;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using Group = Telerik.Reporting.Group;
using Report = Telerik.Reporting.Report;
using SubReport = Telerik.Reporting.SubReport;

namespace GSC.Reports.General
{
    public class ReportRepository : IReportRepository
    {
        private ConnectionStrings AppSettings { get; set; }
        IHostingEnvironment _hostingEnvironment;

        public ReportRepository(IHostingEnvironment hostingEnvironment, IOptions<ConnectionStrings> settings)
        {
            AppSettings = settings.Value;
            _hostingEnvironment = hostingEnvironment;
        }



        //public FileStreamResult PolicyById(int id, string reportName)
        //{
        //    SqlDataSourceParameterCollection parameter = new SqlDataSourceParameterCollection();
        //    parameter.Add(new SqlDataSourceParameter("@policyId", DbType.Int32, id));
        //    SqlDataSource sqlDataSource;
        //    sqlDataSource = GetSqlDataSource("getPolicyQuote", parameter);
        //    return ReportGenerate(reportName, sqlDataSource);
        //}

        public SqlDataSource GetSqlDataSource(string storedProcedure, SqlDataSourceParameterCollection parameter)
        {
            SqlDataSource sqlDataSource = new SqlDataSource();
            sqlDataSource.ConnectionString = AppSettings.dbConnectionString;
            sqlDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
            sqlDataSource.SelectCommand = storedProcedure;
            sqlDataSource.Parameters.AddRange(parameter);

            return sqlDataSource;
        }

        public FileStreamResult ReportGenerate(string reportPath, SqlDataSource sqlDataSource)
        {
            reportPath = _hostingEnvironment.ContentRootPath + reportPath + ".trdp";
            var reportProcessor = new ReportProcessor();

            var reportPackager = new ReportPackager();
            using (var sourceStream = File.OpenRead(reportPath))
            {
                var report = (Report)reportPackager.UnpackageDocument(sourceStream);
                report.DataSource = sqlDataSource;
                Group group = new Group();
                group.Name = "groupheaderfooter";

                SubReport groupHeaderReport = new SubReport();
                SubReport groupFooterReport = new SubReport();

                GroupFooterSection groupFooterSection = new GroupFooterSection();
                GroupHeaderSection groupHeaderSection = new GroupHeaderSection();

                groupHeaderReport.ReportSource = new UriReportSource { Uri = _hostingEnvironment.ContentRootPath + "\\Reports\\shared\\Header.trdp" };
                groupFooterReport.ReportSource = new UriReportSource { Uri = _hostingEnvironment.ContentRootPath + "\\Reports\\shared\\Footer.trdp" };

                
                groupHeaderSection.PrintOnEveryPage = true;
                groupFooterSection.PrintOnEveryPage = true;
                groupFooterSection.PrintAtBottom = true;

                groupFooterSection.Items.Add(groupFooterReport);
                groupHeaderSection.Items.Add(groupHeaderReport);

                group.GroupFooter = groupFooterSection;
                group.GroupHeader = groupHeaderSection;
                report.Groups.Insert(0,group);

                RenderingResult result = reportProcessor.RenderReport("PDF", report, new Hashtable());
                return new FileStreamResult(new MemoryStream(result.DocumentBytes), "application/pdf");
            }
        }


    }
}