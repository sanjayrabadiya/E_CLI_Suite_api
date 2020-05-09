using System;
using System.Collections;
using System.IO;
using GSC.Data.Dto.Configuration;
using GSC.Helper;
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
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ReportBaseRepository(IHostingEnvironment hostingEnvironment, IConfiguration configuration,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        public SqlDataSource DataSource(string strSql, SqlDataSourceParameterCollection parameter)
        {
            var sqlDataSource = new SqlDataSource();
            sqlDataSource.ConnectionString = _configuration.GetConnectionString("dbConnectionString");
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

        public FileStreamResult ReportRunNew(string reportPath, SqlDataSource sqlDataSource, ReportSettingNew reportSettingNew, CompanyData companyData)
        {
            reportPath = _hostingEnvironment.ContentRootPath + "\\Report\\" + reportPath + ".trdp";

            var reportProcessor = new ReportProcessor();

            IsCompanyLogo = reportSettingNew.IsCompanyLogo;
            IsClientLogo = reportSettingNew.IsClientLogo;
            var reportPackager = new ReportPackager();
            using (var sourceStream = File.OpenRead(reportPath))
            {
                var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);
                report.DataSource = sqlDataSource;
                Group group = new Group();
                group.Name = "groupheaderfooter";

                SubReport groupHeaderReport = new SubReport();
                SubReport groupPresentationReport = new SubReport();
                SubReport groupFooterReport = new SubReport();

                GroupFooterSection groupFooterSection = new GroupFooterSection();
                GroupHeaderSection groupHeaderSection = new GroupHeaderSection();

                if (reportSettingNew.PdfStatus == 1)
                    groupHeaderReport.ReportSource = new UriReportSource { Uri = _hostingEnvironment.ContentRootPath + "\\Report\\shared\\Header.trdp" };
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
                rpIsInitials.AllowNull = false;
                rpIsInitials.Value = reportSettingNew.IsInitial;
                rpIsInitials.Visible = true;
                report.ReportParameters.Add(rpIsInitials);

                // Subject Number
                Telerik.Reporting.ReportParameter rpSubNo = new Telerik.Reporting.ReportParameter();
                rpSubNo.Name = "IsSubjectNumber";
                rpSubNo.Text = "";
                rpSubNo.Type = Telerik.Reporting.ReportParameterType.Boolean;
                rpSubNo.AllowBlank = false;
                rpSubNo.AllowNull = false;
                rpSubNo.Value = reportSettingNew.IsSubjectNumber;
                rpSubNo.Visible = true;
                report.ReportParameters.Add(rpSubNo);

                //Screen Number
                Telerik.Reporting.ReportParameter rpScreenNumber = new Telerik.Reporting.ReportParameter();
                rpScreenNumber.Name = "IsScreenNumber";
                rpScreenNumber.Text = "";
                rpScreenNumber.Type = Telerik.Reporting.ReportParameterType.Boolean;
                rpScreenNumber.AllowBlank = false;
                rpScreenNumber.AllowNull = false;
                rpScreenNumber.Value = reportSettingNew.IsScreenNumber;
                rpScreenNumber.Visible = true;
                report.ReportParameters.Add(rpScreenNumber);

                //Sponsor Number
                Telerik.Reporting.ReportParameter rpSponsorNumber = new Telerik.Reporting.ReportParameter();
                rpSponsorNumber.Name = "IsSponsorNumber";
                rpSponsorNumber.Text = "";
                rpSponsorNumber.Type = Telerik.Reporting.ReportParameterType.Boolean;
                rpSponsorNumber.AllowBlank = false;
                rpSponsorNumber.AllowNull = false;
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
                rpIsComLogo.Value = companyData.IsComLogo;
                rpIsComLogo.Visible = true;
                report.ReportParameters.Add(rpIsComLogo);

                // IsClientLogo
                Telerik.Reporting.ReportParameter rpIsClientLogo = new Telerik.Reporting.ReportParameter();
                rpIsClientLogo.Name = "IsClientLogo";
                rpIsClientLogo.Text = "";
                rpIsClientLogo.Type = Telerik.Reporting.ReportParameterType.Boolean;
                rpIsClientLogo.AllowBlank = false;
                rpIsClientLogo.AllowNull = true;
                rpIsClientLogo.Value = companyData.IsClientLogo;
                rpIsClientLogo.Visible = true;
                report.ReportParameters.Add(rpIsClientLogo);

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
                rpIsSignature.AllowNull = false;
                rpIsSignature.Value = companyData.IsSignature;
                rpIsSignature.Visible = true;
                report.ReportParameters.Add(rpIsSignature);

                // Username
                Telerik.Reporting.ReportParameter rpUsername = new Telerik.Reporting.ReportParameter();
                rpUsername.Name = "Username";
                rpUsername.Text = "";
                rpUsername.Type = Telerik.Reporting.ReportParameterType.String;
                rpUsername.AllowBlank = false;
                rpUsername.AllowNull = false;
                rpUsername.Value = companyData.Username;
                rpUsername.Visible = true;
                report.ReportParameters.Add(rpUsername);

                // Datetime
                Telerik.Reporting.ReportParameter rpDatetime = new Telerik.Reporting.ReportParameter();
                rpDatetime.Name = "DatetimeW";
                rpDatetime.Text = "";
                rpDatetime.Type = Telerik.Reporting.ReportParameterType.DateTime;
                rpDatetime.AllowBlank = false;
                rpDatetime.AllowNull = false;
                rpDatetime.Value = System.DateTime.Now;
                rpDatetime.Visible = true;
                report.ReportParameters.Add(rpDatetime);
                #endregion

                groupFooterSection.Items.Add(groupFooterReport);
                group.GroupFooter = groupFooterSection;
                if (reportSettingNew.PdfStatus == 1)
                {
                    groupHeaderSection.Items.Add(groupHeaderReport);
                    group.GroupHeader = groupHeaderSection;
                }
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

                report.PageSettings.Margins.Left = Telerik.Reporting.Drawing.Unit.Inch(Convert.ToDouble(reportSettingNew.LeftMargin));
                report.PageSettings.Margins.Right = Telerik.Reporting.Drawing.Unit.Inch(Convert.ToDouble(reportSettingNew.RightMargin));
                report.PageSettings.Margins.Top = Telerik.Reporting.Drawing.Unit.Inch(Convert.ToDouble(reportSettingNew.TopMargin));
                report.PageSettings.Margins.Bottom = Telerik.Reporting.Drawing.Unit.Inch(Convert.ToDouble(reportSettingNew.BottomMargin));

                RenderingResult result = reportProcessor.RenderReport("PDF", report, new Hashtable());

                return new FileStreamResult(new MemoryStream(result.DocumentBytes), "application/pdf");
            }
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
    }
}