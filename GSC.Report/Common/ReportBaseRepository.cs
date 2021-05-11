using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Common;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Custom;
using GSC.Data.Dto.Report.Pdf;
using GSC.Data.Entities.Custom;
using GSC.Data.Entities.Report;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Reports;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using Group = Telerik.Reporting.Group;
using SubReport = Telerik.Reporting.SubReport;

namespace GSC.Report.Common
{
    public class ReportBaseRepository : IReportBaseRepository
    {
        bool? IsCompanyLogo;
        bool? IsClientLogo;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        private readonly IGSCContext _context;

        public ReportBaseRepository(IHostingEnvironment hostingEnvironment,
            IJwtTokenAccesser jwtTokenAccesser, IJobMonitoringRepository jobMonitoringRepository,
             IGSCContext context)
        {
            _hostingEnvironment = hostingEnvironment;
            _jwtTokenAccesser = jwtTokenAccesser;
            _jobMonitoringRepository = jobMonitoringRepository;
            _context = context;
        }


        public SqlDataSource DataSource(string strSql, SqlDataSourceParameterCollection parameter)
        {
            var sqlDataSource = new SqlDataSource();
            sqlDataSource.ConnectionString = _context.GetConnectionString();
            //sqlDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
            sqlDataSource.SelectCommandType = SqlDataSourceCommandType.Text;
            sqlDataSource.SelectCommand = strSql;
            if (parameter != null)
                sqlDataSource.Parameters.AddRange(parameter);

            return sqlDataSource;
        }

        public FileStreamResult ReportRun(string reportPath, SqlDataSource sqlDataSource)
        {
            reportPath = _hostingEnvironment.ContentRootPath + "\\Report\\" + reportPath + ".trdp";
            var reportProcessor = new ReportProcessor();

            var reportPackager = new ReportPackager();
            using (var sourceStream = File.OpenRead(reportPath))
            {
                var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);
                report.DataSource = sqlDataSource;
                var group = new Group();
                group.Name = "groupheaderfooter";

                var groupHeaderReport = new SubReport();
                var groupFooterReport = new SubReport();

                var groupFooterSection = new GroupFooterSection();
                var groupHeaderSection = new GroupHeaderSection();

                groupHeaderReport.ReportSource = new UriReportSource
                { Uri = _hostingEnvironment.ContentRootPath + "\\Report\\shared\\Header.trdp" };
                groupFooterReport.ReportSource = new UriReportSource
                { Uri = _hostingEnvironment.ContentRootPath + "\\Report\\shared\\Footer.trdp" };
                groupHeaderReport.NeedDataSource += GroupFooterReport_NeedDataSource;

                groupHeaderSection.PrintOnEveryPage = true;
                groupFooterSection.PrintOnEveryPage = true;
                groupFooterSection.PrintAtBottom = true;

                groupFooterSection.Items.Add(groupFooterReport);
                groupHeaderSection.Items.Add(groupHeaderReport);

                group.GroupFooter = groupFooterSection;
                group.GroupHeader = groupHeaderSection;
                report.Groups.Insert(0, group);

                var result = reportProcessor.RenderReport("PDF", report, new Hashtable());
                return new FileStreamResult(new MemoryStream(result.DocumentBytes), "application/pdf");
            }
        }

        private string GetCompany()
        {
            return @"SELECT Company.CompanyName,Company.Phone1,Company.Phone2,Location.Address,
                State.StateName,City.CityName,Country.CountryName,
                CASE WHEN ISNULL(Company.Logo,'')<>'' THEN + UploadSetting.ImageUrl + Company.Logo END Logo,
                CASE WHEN ISNULL(Client.Logo,'')<>'' THEN + UploadSetting.ImageUrl + Client.Logo  END ClientLogo
                FROM Company
                LEFT OUTER JOIN Location ON Location.Id = Company.LocationId
                LEFT OUTER JOIN State ON State.Id = Location.StateId
                LEFT OUTER JOIN City ON City.Id = Location.CityId
                LEFT OUTER JOIN Country ON Country.Id = Location.CountryId
                LEFT OUTER JOIN UploadSetting ON UploadSetting.CompanyId = Company.Id
                LEFT OUTER JOIN Client ON Client.CompanyId = Company.Id

                WHERE Company.Id=" +
                   Convert.ToString(_jwtTokenAccesser.CompanyId == 0 ? 1 : _jwtTokenAccesser.CompanyId);
        }

        public FileStreamResult ReportRunNew(string reportPath, SqlDataSource sqlDataSource, ReportSettingNew reportSettingNew, CompanyDataDto companyData, FileSaveInfo fileInfo)
        {
            try
            {
                reportPath = _hostingEnvironment.ContentRootPath + "\\Report\\" + reportPath + ".trdp";

                var reportProcessor = new ReportProcessor();

                IsCompanyLogo = reportSettingNew.IsCompanyLogo;
                IsClientLogo = reportSettingNew.IsClientLogo;
                var reportPackager = new ReportPackager();
                using (var sourceStream = File.OpenRead(reportPath))
                {

                    #region Check Company Logo exist or not
                    if (string.IsNullOrEmpty(companyData.Logo))
                    {
                        //FileInfo file = new FileInfo(companyData.Logo);
                        //var aa = File.Exists(@"" + companyData.Logo);
                        //if (File.Exists(companyData.Logo))
                        //{
                        companyData.Logo = "";
                        companyData.IsComLogo = null;
                        //}
                    }
                    #endregion

                    #region Check Client Logo exist or not
                    if (string.IsNullOrEmpty(companyData.ClientLogo))
                    {
                        //FileInfo ClientFile = new FileInfo(companyData.ClientLogo);
                        //if (ClientFile.Exists.Equals(false))
                        //{
                        companyData.ClientLogo = "";
                        companyData.IsClientLogo = null;
                        //}
                    }
                    #endregion
                    var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);
                    report.DataSource = sqlDataSource;
                    Group group = new Group();
                    group.Name = "groupheaderfooter";

                    SubReport groupHeaderReport = new SubReport();
                    SubReport groupPresentationReport = new SubReport();
                    SubReport groupFooterReport = new SubReport();

                    GroupFooterSection groupFooterSection = new GroupFooterSection();
                    GroupHeaderSection groupHeaderSection = new GroupHeaderSection();

                    groupFooterReport.ReportSource = new UriReportSource { Uri = _hostingEnvironment.ContentRootPath + "\\Report\\shared\\Footer.trdp" };
                    groupHeaderReport.NeedDataSource += GroupFooterReport_NeedDataSource;
                    groupHeaderSection.PrintOnEveryPage = true;
                    groupFooterSection.PrintOnEveryPage = true;
                    groupFooterSection.PrintAtBottom = true;

                    #region Report Parameter
                    // IsInitials
                    Telerik.Reporting.ReportParameter rpIsInitials = new Telerik.Reporting.ReportParameter();
                    rpIsInitials.Name = "IsInitials";
                    rpIsInitials.Text = "";
                    rpIsInitials.Type = Telerik.Reporting.ReportParameterType.Boolean;
                    rpIsInitials.AllowBlank = false;
                    rpIsInitials.AllowNull = true;
                    rpIsInitials.Value = reportSettingNew.IsInitial;
                    rpIsInitials.Visible = true;
                    report.ReportParameters.Add(rpIsInitials);

                    // Subject Number
                    Telerik.Reporting.ReportParameter rpSubNo = new Telerik.Reporting.ReportParameter();
                    rpSubNo.Name = "IsSubjectNumber";
                    rpSubNo.Text = "";
                    rpSubNo.Type = Telerik.Reporting.ReportParameterType.Boolean;
                    rpSubNo.AllowBlank = false;
                    rpSubNo.AllowNull = true;
                    rpSubNo.Value = reportSettingNew.IsSubjectNumber;
                    rpSubNo.Visible = true;
                    report.ReportParameters.Add(rpSubNo);

                    //Screen Number
                    Telerik.Reporting.ReportParameter rpScreenNumber = new Telerik.Reporting.ReportParameter();
                    rpScreenNumber.Name = "IsScreenNumber";
                    rpScreenNumber.Text = "";
                    rpScreenNumber.Type = Telerik.Reporting.ReportParameterType.Boolean;
                    rpScreenNumber.AllowBlank = false;
                    rpScreenNumber.AllowNull = true;
                    rpScreenNumber.Value = reportSettingNew.IsScreenNumber;
                    rpScreenNumber.Visible = true;
                    report.ReportParameters.Add(rpScreenNumber);

                    //Sponsor Number
                    Telerik.Reporting.ReportParameter rpSponsorNumber = new Telerik.Reporting.ReportParameter();
                    rpSponsorNumber.Name = "IsSponsorNumber";
                    rpSponsorNumber.Text = "";
                    rpSponsorNumber.Type = Telerik.Reporting.ReportParameterType.Boolean;
                    rpSponsorNumber.AllowBlank = false;
                    rpSponsorNumber.AllowNull = true;
                    rpSponsorNumber.Value = reportSettingNew.IsSponsorNumber;
                    rpSponsorNumber.Visible = true;
                    report.ReportParameters.Add(rpSponsorNumber);
                    #endregion

                    #region Company Data Parameter
                    // IsComLogo
                    Telerik.Reporting.ReportParameter rpIsComLogo = new Telerik.Reporting.ReportParameter();
                    rpIsComLogo.Name = "IsComLogo";
                    rpIsComLogo.Text = "";
                    rpIsComLogo.Type = Telerik.Reporting.ReportParameterType.Boolean;
                    rpIsComLogo.AllowBlank = false;
                    rpIsComLogo.AllowNull = true;
                    rpIsComLogo.Value = string.IsNullOrEmpty(companyData.IsComLogo) ? "false" : companyData.IsComLogo;
                    rpIsComLogo.Visible = true;
                    report.ReportParameters.Add(rpIsComLogo);

                    // IsClientLogo
                    Telerik.Reporting.ReportParameter rpIsClientLogo = new Telerik.Reporting.ReportParameter();
                    rpIsClientLogo.Name = "IsClientLogo";
                    rpIsClientLogo.Text = "";
                    rpIsClientLogo.Type = Telerik.Reporting.ReportParameterType.Boolean;
                    rpIsClientLogo.AllowBlank = false;
                    rpIsClientLogo.AllowNull = true;
                    rpIsClientLogo.Value = string.IsNullOrEmpty(companyData.IsClientLogo) ? "false" : companyData.IsClientLogo;
                    rpIsClientLogo.Visible = true;
                    report.ReportParameters.Add(rpIsClientLogo);

                    // IsSiteCode
                    Telerik.Reporting.ReportParameter rpIsSiteCode = new Telerik.Reporting.ReportParameter();
                    rpIsSiteCode.Name = "IsSiteCode";
                    rpIsSiteCode.Text = "";
                    rpIsSiteCode.Type = Telerik.Reporting.ReportParameterType.Boolean;
                    rpIsSiteCode.AllowBlank = false;
                    rpIsSiteCode.AllowNull = true;
                    rpIsSiteCode.Value = string.IsNullOrEmpty(companyData.IsSiteCode) ? "false" : companyData.IsSiteCode;
                    rpIsSiteCode.Visible = true;
                    report.ReportParameters.Add(rpIsSiteCode);

                    //CompanyName
                    Telerik.Reporting.ReportParameter rpCompanyName = new Telerik.Reporting.ReportParameter();
                    rpCompanyName.Name = "CompanyName";
                    rpCompanyName.Text = "";
                    rpCompanyName.Type = Telerik.Reporting.ReportParameterType.String;
                    rpCompanyName.AllowBlank = false;
                    rpCompanyName.AllowNull = true;
                    rpCompanyName.Value = companyData.CompanyName;
                    rpCompanyName.Visible = true;
                    report.ReportParameters.Add(rpCompanyName);

                    #region Extra Field
                    ////Phone1
                    //Telerik.Reporting.ReportParameter rpPhone1 = new Telerik.Reporting.ReportParameter();
                    //rpPhone1.Name = "Phone1";
                    //rpPhone1.Text = "";
                    //rpPhone1.Type = Telerik.Reporting.ReportParameterType.String;
                    //rpPhone1.AllowBlank = false;
                    //rpPhone1.AllowNull = false;
                    //rpPhone1.Value = companyData.Phone1;
                    //rpPhone1.Visible = true;
                    //report.ReportParameters.Add(rpPhone1);


                    //// Phone2
                    //Telerik.Reporting.ReportParameter rpPhone2 = new Telerik.Reporting.ReportParameter();
                    //rpPhone2.Name = "Phone2";
                    //rpPhone2.Text = "";
                    //rpPhone2.Type = Telerik.Reporting.ReportParameterType.String;
                    //rpPhone2.AllowBlank = false;
                    //rpPhone2.AllowNull = false;
                    //rpPhone2.Value = companyData.Phone2 != null ? companyData.Phone2 :  ;
                    //rpPhone2.Visible = true;
                    //report.ReportParameters.Add(rpPhone2);

                    //// Address
                    //Telerik.Reporting.ReportParameter rpAddress = new Telerik.Reporting.ReportParameter();
                    //rpAddress.Name = "Address";
                    //rpAddress.Text = "";
                    //rpAddress.Type = Telerik.Reporting.ReportParameterType.String;
                    //rpAddress.AllowBlank = false;
                    //rpAddress.AllowNull = false;
                    //rpAddress.Value = companyData.Address;
                    //rpAddress.Visible = true;
                    //report.ReportParameters.Add(rpAddress);

                    ////StateName
                    //Telerik.Reporting.ReportParameter rpStateName = new Telerik.Reporting.ReportParameter();
                    //rpStateName.Name = "StateName";
                    //rpStateName.Text = "";
                    //rpStateName.Type = Telerik.Reporting.ReportParameterType.String;
                    //rpStateName.AllowBlank = false;
                    //rpStateName.AllowNull = false;
                    //rpStateName.Value = companyData.StateName;
                    //rpStateName.Visible = true;
                    //report.ReportParameters.Add(rpStateName);

                    ////CityName
                    //Telerik.Reporting.ReportParameter rpCityName = new Telerik.Reporting.ReportParameter();
                    //rpCityName.Name = "CityName";
                    //rpCityName.Text = "";
                    //rpCityName.Type = Telerik.Reporting.ReportParameterType.String;
                    //rpCityName.AllowBlank = false;
                    //rpCityName.AllowNull = false;
                    //rpCityName.Value = companyData.CityName;
                    //rpCityName.Visible = true;
                    //report.ReportParameters.Add(rpCityName);


                    ////CountryName
                    //Telerik.Reporting.ReportParameter rpCountryName = new Telerik.Reporting.ReportParameter();
                    //rpCountryName.Name = "CountryName";
                    //rpCountryName.Text = "";
                    //rpCountryName.Type = Telerik.Reporting.ReportParameterType.String;
                    //rpCountryName.AllowBlank = false;
                    //rpCountryName.AllowNull = false;
                    //rpCountryName.Value = companyData.CountryName;
                    //rpCountryName.Visible = true;
                    //report.ReportParameters.Add(rpCountryName);
                    #endregion

                    //Logo
                    Telerik.Reporting.ReportParameter rpLogo = new Telerik.Reporting.ReportParameter();
                    rpLogo.Name = "Logo";
                    rpLogo.Text = "";
                    rpLogo.Type = Telerik.Reporting.ReportParameterType.String;
                    rpLogo.AllowBlank = false;
                    rpLogo.AllowNull = true;
                    rpLogo.Value = companyData.Logo;
                    rpLogo.Visible = true;
                    report.ReportParameters.Add(rpLogo);

                    //ClientLogo
                    Telerik.Reporting.ReportParameter rpClientLogo = new Telerik.Reporting.ReportParameter();
                    rpClientLogo.Name = "ClientLogo";
                    rpClientLogo.Text = "";
                    rpClientLogo.Type = Telerik.Reporting.ReportParameterType.String;
                    rpClientLogo.AllowBlank = false;
                    rpClientLogo.AllowNull = true;
                    rpClientLogo.Value = companyData.ClientLogo;
                    rpClientLogo.Visible = true;
                    report.ReportParameters.Add(rpClientLogo);
                    #endregion

                    #region WorkflowData
                    // IsSignature
                    Telerik.Reporting.ReportParameter rpIsSignature = new Telerik.Reporting.ReportParameter();
                    rpIsSignature.Name = "IsSignature";
                    rpIsSignature.Text = "";
                    rpIsSignature.Type = Telerik.Reporting.ReportParameterType.Boolean;
                    rpIsSignature.AllowBlank = false;
                    rpIsSignature.AllowNull = true;
                    rpIsSignature.Value = string.IsNullOrEmpty(companyData.IsSignature) ? "false" : companyData.IsSignature;
                    rpIsSignature.Visible = true;
                    report.ReportParameters.Add(rpIsSignature);

                    // Username
                    Telerik.Reporting.ReportParameter rpUsername = new Telerik.Reporting.ReportParameter();
                    rpUsername.Name = "Username";
                    rpUsername.Text = "";
                    rpUsername.Type = Telerik.Reporting.ReportParameterType.String;
                    rpUsername.AllowBlank = false;
                    rpUsername.AllowNull = true;
                    rpUsername.Value = companyData.Username;
                    rpUsername.Visible = true;
                    report.ReportParameters.Add(rpUsername);

                    // Datetime
                    Telerik.Reporting.ReportParameter rpDatetime = new Telerik.Reporting.ReportParameter();
                    rpDatetime.Name = "DatetimeW";
                    rpDatetime.Text = "";
                    rpDatetime.Type = Telerik.Reporting.ReportParameterType.String;
                    rpDatetime.AllowBlank = true;
                    rpDatetime.AllowNull = true;
                    rpDatetime.Value = reportSettingNew.ClientDateTime;
                    rpDatetime.Visible = true;
                    report.ReportParameters.Add(rpDatetime);
                    #endregion

                    groupFooterSection.Items.Add(groupFooterReport);
                    group.GroupFooter = groupFooterSection;
                    //if (reportSettingNew.PdfStatus == 1)
                    //{
                    //    groupHeaderSection.Items.Add(groupHeaderReport);
                    //    group.GroupHeader = groupHeaderSection;
                    //}
                    report.Groups.Insert(0, group);

                    Telerik.Reporting.Drawing.TextWatermark textWatermark1 = new Telerik.Reporting.Drawing.TextWatermark();
                    textWatermark1.Font.Bold = true;
                    textWatermark1.Font.Italic = false;
                    textWatermark1.Font.Name = "Arial";
                    textWatermark1.Font.Size = Telerik.Reporting.Drawing.Unit.Point(30D);
                    textWatermark1.Font.Strikeout = false;
                    textWatermark1.Font.Underline = false;
                    textWatermark1.Orientation = Telerik.Reporting.Drawing.WatermarkOrientation.Diagonal;
                    textWatermark1.Position = Telerik.Reporting.Drawing.WatermarkPosition.Behind;
                    textWatermark1.PrintOnFirstPage = true;
                    textWatermark1.PrintOnLastPage = true;
                    textWatermark1.Text = reportSettingNew.PdfType == 1 ? "Draft" : "";
                    textWatermark1.Opacity = 0.3D;
                    report.PageSettings.Watermarks.Add(textWatermark1);

                    if (reportSettingNew.LeftMargin != null)
                        report.PageSettings.Margins.Left = Telerik.Reporting.Drawing.Unit.Inch(Convert.ToDouble(reportSettingNew.LeftMargin));
                    if (reportSettingNew.RightMargin != null)
                        report.PageSettings.Margins.Right = Telerik.Reporting.Drawing.Unit.Inch(Convert.ToDouble(reportSettingNew.RightMargin));
                    if (reportSettingNew.TopMargin != null)
                        report.PageSettings.Margins.Top = Telerik.Reporting.Drawing.Unit.Inch(Convert.ToDouble(reportSettingNew.TopMargin));
                    if (reportSettingNew.BottomMargin != null)
                        report.PageSettings.Margins.Bottom = Telerik.Reporting.Drawing.Unit.Inch(Convert.ToDouble(reportSettingNew.BottomMargin));

                    RenderingResult result = reportProcessor.RenderReport("PDF", report, new Hashtable());

                    #region filesave
                    string fileName = fileInfo.FileName + "." + result.Extension;
                    string filePath = string.Empty;
                    if (reportSettingNew.PdfStatus == DossierPdfStatus.Blank)
                        filePath = System.IO.Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName, fileName);
                    else
                        filePath = System.IO.Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName, fileInfo.ChildFolderName, fileName);

                    bool exists = Directory.Exists(filePath);
                    if (!exists)
                        if (reportSettingNew.PdfStatus == DossierPdfStatus.Blank)
                            System.IO.Directory.CreateDirectory(Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName));
                        else
                            System.IO.Directory.CreateDirectory(Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName, fileInfo.ChildFolderName));

                    using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
                    }
                    #endregion
                    return new FileStreamResult(new MemoryStream(result.DocumentBytes), "application/pdf");
                }
            }
            catch (Exception ex)
            {
                string path = System.IO.Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, Enum.GetName(typeof(JobStatusType), JobStatusType.Log), fileInfo.ParentFolderName, fileInfo.ParentFolderName + ".txt");
                WriteLog(ex.Message, path, null);
                throw;
            }
        }

        public void WriteLog(string strLog, string FolderPath, JobMonitoring jobMonitoring)
        {
            string logFilePath = FolderPath;
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            using (FileStream fileStream = new FileStream(logFilePath, FileMode.Append))
            {
                using (StreamWriter log = new StreamWriter(fileStream))
                {
                    log.WriteLine(strLog);
                }
            }
            CompleteJobMonitoring(jobMonitoring);
        }

        public string CompleteJobMonitoring(JobMonitoring jobMonitoring)
        {
            #region Update JobMonitoring            
            _jobMonitoringRepository.Update(jobMonitoring);
            if (_context.Save() <= 0) throw new Exception("updating Job Monitoring failed on save.");
            #endregion
            return "";
        }

        private void GroupFooterReport_NeedDataSource(object sender, EventArgs e)
        {
            (sender as Telerik.Reporting.Processing.SubReport).InnerReport.DataSource = DataSource(GetCompanyNew(IsCompanyLogo, IsClientLogo), null);
        }

        private string GetCompanyNew(bool? IsCompanyLogo, bool? IsClientLogo)
        {
            var assa =
            @"SELECT   '" + IsCompanyLogo.ToString().ToLower() + "' AS IsComLogo,'" + IsClientLogo.ToString().ToLower() + "' AS IsClientLogo," +
            "Company.CompanyName,Company.Phone1,Company.Phone2,Location.Address, " +
            "State.StateName,City.CityName,Country.CountryName," +
            "CASE WHEN ISNULL(Company.Logo,'')<>'' THEN + UploadSetting.ImageUrl + Company.Logo END Logo," +
            "CASE WHEN ISNULL(Client.Logo,'')<>'' THEN + UploadSetting.ImageUrl + Client.Logo  END ClientLogo " +
            "FROM Company " +
            "LEFT OUTER JOIN Location ON Location.Id = Company.LocationId " +
            "LEFT OUTER JOIN State ON State.Id = Location.StateId " +
            "LEFT OUTER JOIN City ON City.Id = Location.CityId " +
            "LEFT OUTER JOIN Country ON Country.Id = Location.CountryId " +
            "LEFT OUTER JOIN UploadSetting ON UploadSetting.CompanyId = Company.Id " +
            "LEFT OUTER JOIN Client ON Client.CompanyId = Company.Id " +
            "WHERE Company.Id=" + Convert.ToString(_jwtTokenAccesser.CompanyId == 0 ? 1 : _jwtTokenAccesser.CompanyId);
            return assa;
        }

        //private void EventsReport_NewPage(object sender, PageEventArgs e)
        //{
        //    string text = string.Format("Page {0} of {1}", e.PageNumber, e.PageCount);
        //    Telerik.Reporting.Processing.Report report = (Telerik.Reporting.Processing.Report)sender;
        //    Telerik.Reporting.Processing.ReportItemBase[] items = report.Items.Find("txtPageNumber", true);
        //    if (items.Length > 0)
        //    {
        //        Processing.TextBox pageNo = items[0] as Processing.TextBox;
        //        if (null != pageNo)
        //        {
        //            pageNo.Text = text;
        //        }
        //    }
        //}

        //public List<DossierPdfDataDto> GetpdfReportData()
        //{
        //    SqlParameter parameter = new SqlParameter("@projectId",13);
        //    string sqlqry = @"select 
        //                      pg.ProjectId
        //                      ,pdv.ProjectDesignPeriodId
        //                      ,pdt.ProjectDesignVisitId
        //                      ,pdva.ProjectDesignTemplateId
        //                      ,p.ProjectCode,
        //                      p.ProjectName,
        //                      pdv.DisplayName,
        //                      pdt.TemplateCode,
        //                      pdt.TemplateName,
        //                      pdva.VariableName 
        //                      ,d.DomainName
        //                      ,d.DomainCode
        //                      FROM Project p 
        //                      INNER JOIN  ProjectDesign pg on pg.ProjectId=p.Id
        //                      INNER JOIN ProjectDesignPeriod pdp on pg.Id=pdp.ProjectDesignId
        //                      INNER JOIN ProjectDesignVisit pdv on pdv.ProjectDesignPeriodId=pdp.Id AND pdv.DeletedDate IS NULL 
        //                      INNER JOIN ProjectDesignTemplate pdt on pdt.ProjectDesignVisitId=pdv.Id AND pdt.DeletedDate IS NULL
        //                      INNER JOIN Domain d on d.Id=pdt.DomainId
        //                      INNER JOIN ProjectDesignVariable pdva on pdva.ProjectDesignTemplateId=pdt.Id
        //                      WHERE ProjectId=13  
        //                      Order By pdv.DesignOrder,pdt.DesignOrder";
        //    var finaldata = _context.FromSql <DossierPdfDataDto>(sqlqry).ToList();
        //    return finaldata;
        //}

        public List<DossierReportDto> GetBlankPdfData(ReportSettingNew reportSetting)
        {

            var finaldata = _context.ProjectDesign.Where(x => x.ProjectId == reportSetting.ProjectId).Select(x => new DossierReportDto
            {
                ProjectDetails = new ProjectDetails { ProjectCode = x.Project.ProjectCode, ProjectName = x.Project.ProjectName, ClientId = x.Project.ClientId },
                Period = x.ProjectDesignPeriods.Where(a => a.DeletedDate == null).Select(a => new ProjectDesignPeriodReportDto
                {
                    DisplayName = a.DisplayName,
                    Visit = a.VisitList.Where(b => b.DeletedDate == null).Select(b => new ProjectDesignVisitList
                    {
                        DisplayName = b.DisplayName,
                        DesignOrder = b.DesignOrder,
                        ProjectDesignTemplatelist = b.Templates.Where(n => n.DeletedDate == null && (reportSetting.NonCRF == true ? n.VariableTemplate.ActivityMode == ActivityMode.Generic || n.VariableTemplate.ActivityMode == ActivityMode.SubjectSpecific : n.VariableTemplate.ActivityMode == ActivityMode.SubjectSpecific)).Select(n => new ProjectDesignTemplatelist
                        {
                            TemplateCode = n.TemplateCode,
                            TemplateName = n.TemplateName,
                            DesignOrder = n.DesignOrder,
                            Domain = new DomainReportDto { DomainCode = n.Domain.DomainCode, DomainName = n.Domain.DomainName },
                            TemplateNotes = n.ProjectDesignTemplateNote.Where(tn => tn.DeletedDate == null).Select(tn => new ProjectDesignTemplateNoteReportDto { Notes = tn.Note, IsPreview = tn.IsPreview }).ToList(),
                            ProjectDesignVariable = n.Variables.Where(v => v.DeletedDate == null).Select(v => new ProjectDesignVariableReportDto
                            {
                                Id = v.Id,
                                VariableName = v.VariableName,
                                VariableCode = v.VariableCode,
                                DesignOrder = v.DesignOrder,
                                IsNa = v.IsNa,
                                CollectionSource = v.CollectionSource,
                                Annotation = v.Annotation,
                                CollectionAnnotation = v.CollectionAnnotation,
                                Note = v.Note,
                                DefaultValue = v.DefaultValue,
                                LowRangeValue = v.LowRangeValue,
                                HighRangeValue = v.HighRangeValue,
                                Unit = new UnitReportDto { UnitName = v.Unit.UnitName },
                                Values = v.Values.Where(vd => vd.DeletedDate == null).Select(vd => new ProjectDesignVariableValueReportDto { Id = vd.Id, ValueName = vd.ValueName, SeqNo = vd.SeqNo, ValueCode = vd.ValueCode, Label = vd.Label }).OrderBy(vd => vd.SeqNo).ToList()
                            }).ToList()
                        }).ToList()
                    }).OrderBy(d => d.DesignOrder).ToList()
                }).ToList()
            }).ToList();
            return finaldata;
        }

        public List<DossierReportDto> GetDataPdfReport(ReportSettingNew reportSetting)
        {

            var finaldata = _context.ScreeningEntry.Where(a => reportSetting.SiteId.Contains(a.ProjectId) && a.DeletedDate == null &&
              (reportSetting.SubjectIds == null || reportSetting.SubjectIds.Select(x => x.Id).ToList().Contains((int)a.RandomizationId)))
              .Select(x => new DossierReportDto
              {
                  ScreeningNumber = x.Randomization.ScreeningNumber,
                  Initial = x.Randomization.Initial,
                  RandomizationNumber = x.Randomization.RandomizationNumber,
                  ProjectDetails = new ProjectDetails
                  {
                      ProjectCode = x.Project.ProjectCode,
                      ProjectName = x.Project.ProjectName,
                      ClientId = x.Project.ClientId
                  },
                  Period = new List<ProjectDesignPeriodReportDto> {
          new ProjectDesignPeriodReportDto {
            DisplayName = x.ProjectDesignPeriod.DisplayName,
              Visit = x.ScreeningVisit.Where(x => x.Status != ScreeningVisitStatus.NotStarted && x.DeletedDate == null && x.ProjectDesignVisit.DeletedDate==null).Select(x => new ProjectDesignVisitList {
                  DisplayName = x.RepeatedVisitNumber==null ?x.ProjectDesignVisit.DisplayName:x.ProjectDesignVisit.DisplayName+"_"+x.RepeatedVisitNumber,
                  DesignOrder = x.ProjectDesignVisit.DesignOrder,
                  ProjectDesignTemplatelist = x.ScreeningTemplates.Where(a => a.Status != ScreeningTemplateStatus.Pending &&
                    a.DeletedDate == null  && a.ProjectDesignTemplate.DeletedDate == null && (reportSetting.NonCRF == true ? a.ProjectDesignTemplate.VariableTemplate.ActivityMode == ActivityMode.Generic || a.ProjectDesignTemplate.VariableTemplate.ActivityMode == ActivityMode.SubjectSpecific : a.ProjectDesignTemplate.VariableTemplate.ActivityMode == ActivityMode.SubjectSpecific)).Select(a => new ProjectDesignTemplatelist {
                    TemplateCode = a.ProjectDesignTemplate.TemplateCode,
                      TemplateName = a.ProjectDesignTemplate.TemplateName,
                      DesignOrder = a.ProjectDesignTemplate.DesignOrder,
                      RepeatSeqNo=a.RepeatSeqNo,
                      Domain = new DomainReportDto {
                        DomainCode = a.ProjectDesignTemplate.Domain.DomainCode, DomainName = a.ProjectDesignTemplate.Domain.DomainName
                      },
                      TemplateNotes = a.ProjectDesignTemplate.ProjectDesignTemplateNote.Select(n => new ProjectDesignTemplateNoteReportDto {
                        Notes = n.Note, IsPreview = n.IsPreview
                      }).ToList(),
                      ProjectDesignVariable = a.ScreeningTemplateValues.Where(s => s.DeletedDate == null && s.ProjectDesignVariable.DeletedDate == null).Select(s => new ProjectDesignVariableReportDto {
                        Id = s.ProjectDesignVariable.Id,
                          VariableName = s.ProjectDesignVariable.VariableName,
                          VariableCode = s.ProjectDesignVariable.VariableCode,
                          DesignOrder = s.ProjectDesignVariable.DesignOrder,
                          IsNa = s.ProjectDesignVariable.IsNa,
                          CollectionSource = s.ProjectDesignVariable.CollectionSource,
                          Annotation=s.ProjectDesignVariable.Annotation,
                          CollectionAnnotation=s.ProjectDesignVariable.CollectionAnnotation,
                          Note=s.ProjectDesignVariable.Note,
                          Unit = new UnitReportDto {
                            UnitName = s.ProjectDesignVariable.Unit.UnitName
                          },
                          Values = s.ProjectDesignVariable.Values.Where(vd => vd.DeletedDate == null).Select(vd => new ProjectDesignVariableValueReportDto {
                            Id = vd.Id, ValueName = vd.ValueName, SeqNo = vd.SeqNo, ValueCode = vd.ValueCode, Label = vd.Label
                          }).ToList(),
                          ScreeningValue= s.Value,
                          ScreeningIsNa=s.IsNa,
                          ScreeningTemplateValueId=s.Id,
                          ValueChild=s.Children.Where(c=>c.DeletedDate==null).Select(c=>new ScreeningTemplateValueChildReportDto{Value=c.Value,ProjectDesignVariableValueId=c.ProjectDesignVariableValueId,ScreeningTemplateValueId=c.ScreeningTemplateValueId,ValueName=c.ProjectDesignVariableValue.ValueName }).ToList()

                      }).ToList(),
                      ScreeningTemplateReview=a.ScreeningTemplateReview.Where(r=>r.DeletedDate==null).Select(r=>new ScreeningTemplateReviewReportDto{
                      ScreeningTemplateId=r.ScreeningTemplateId,
                      ReviewLevel=r.ReviewLevel,
                      RoleId=r.RoleId,
                      RoleName=r.Role.RoleName,
                      CreatedByUser=r.CreatedByUser.UserName,
                      CreatedDate=r.CreatedDate
                      }).ToList()
                  }).ToList()
              }).OrderBy(x=>x.DesignOrder).ToList()

          }
                  }
              }).ToList();
            return finaldata;
        }
    }
}