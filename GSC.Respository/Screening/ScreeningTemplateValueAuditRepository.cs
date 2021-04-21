using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Report;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Reports;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueAuditRepository : GenericRespository<ScreeningTemplateValueAudit>,
        IScreeningTemplateValueAuditRepository
    {
        private static List<string> _months = new List<string>
            {"UNK", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};
        private readonly PdfFont smallfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 6);
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        public ScreeningTemplateValueAuditRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IAppSettingRepository appSettingRepository,
            IUserRepository userRepository, IUploadSettingRepository uploadSettingRepository, IJobMonitoringRepository jobMonitoringRepository,
            IEmailSenderRespository emailSenderRespository)
            : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _appSettingRepository = appSettingRepository;
            _userRepository = userRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _jobMonitoringRepository = jobMonitoringRepository;
            _emailSenderRespository = emailSenderRespository;
        }

        public IList<ScreeningAuditDto> GetAudits(int screeningTemplateValueId)
        {

            return All.Where(x => x.ScreeningTemplateValueId == screeningTemplateValueId).Select(r => new ScreeningAuditDto
            {
                CreatedDate = r.CreatedDate,
                IpAddress = r.IpAddress,
                NewValue = r.Value,
                Note = r.Note,
                OldValue = !string.IsNullOrEmpty(r.Value) && string.IsNullOrEmpty(r.OldValue)
                            ? "Default"
                            : r.OldValue,
                Reason = r.AuditReason.ReasonName,
                ReasonOth = r.ReasonOth,
                Role = r.UserRole,
                TimeZone = r.TimeZone,
                User = r.UserName,
                CollectionSource = r.ScreeningTemplateValue.ProjectDesignVariable.CollectionSource,
                Id = r.Id
            }).OrderByDescending(t => t.Id).ToList();

        }

        public IList<ScreeningAuditDto> GetAuditHistoryByScreeningEntry(int id)
        {
            return All.Where(x => x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntryId == id).Select(r => new ScreeningAuditDto
            {
                CreatedDate = r.CreatedDate,
                IpAddress = r.IpAddress,
                NewValue = r.Value,
                Note = r.Note,
                OldValue = !string.IsNullOrEmpty(r.Value) && string.IsNullOrEmpty(r.OldValue)
                                     ? "Default"
                                     : r.OldValue,
                Reason = r.AuditReason.ReasonName,
                Role = r.UserRole,
                Template = r.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplate.TemplateName,
                TimeZone = r.TimeZone,
                User = r.UserName,
                Variable = r.ScreeningTemplateValue.ProjectDesignVariable.VariableName,
                Visit = r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ProjectDesignVisit.DisplayName +
                Convert.ToString(r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber)
            }).OrderByDescending(t => t.CreatedDate).ToList();
        }

        public void GetDataEntryAuditReportHistory(ProjectDatabaseSearchDto search)
        {
            var query = All.AsQueryable();

            var sites = new List<int>();
            if (search.SiteId != null)
                sites = _context.Project.Where(x => x.Id == search.SiteId).ToList().Select(x => x.Id).ToList();
            else
                sites = _context.Project.Where(x => x.ParentProjectId == search.ParentProjectId && x.IsTestSite == false).ToList().Select(x => x.Id).ToList();

            query = query.Where(x => sites.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
            && x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.DeletedDate == null
            && x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ProjectDesignVisit.DeletedDate == null
             && x.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplate.DeletedDate == null
             && x.ScreeningTemplateValue.ProjectDesignVariable.DeletedDate == null);

            if (search.SubjectIds != null && search.SubjectIds.Length > 0)
                query = query.Where(x => search.SubjectIds.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Id));

            if (search.VisitIds != null && search.VisitIds.Length > 0)
                query = query.Where(x => search.VisitIds.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ProjectDesignVisitId));

            if (search.TemplateIds != null && search.TemplateIds.Length > 0)
                query = query.Where(x => search.VisitIds.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplateId));

            if (search.VariableIds != null && search.VariableIds.Length > 0)
                query = query.Where(x => search.VisitIds.Contains(x.ScreeningTemplateValue.ProjectDesignVariableId));

            GetItems(query, search);
        }

        public void GetItems(IQueryable<ScreeningTemplateValueAudit> query, ProjectDatabaseSearchDto filters)
        {
            var ProjectCode = _context.Project.Find(filters.ParentProjectId).ProjectCode;

            var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            GeneralSettings.TimeFormat = GeneralSettings.TimeFormat.Replace("a", "tt");

            var MainData = query.Select(r => new DataEntryAuditReportDto
            {
                CreatedDate = r.CreatedDate,
                IpAddress = r.IpAddress,
                NewValue = r.Value,
                Note = r.Note,
                OldValue = !string.IsNullOrEmpty(r.Value) && string.IsNullOrEmpty(r.OldValue) ? "Default" : r.OldValue,
                Reason = r.AuditReason.ReasonName,
                ReasonOth = r.ReasonOth,
                Role = r.UserRole,
                Template = r.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplate.TemplateName,
                TimeZone = r.TimeZone,
                User = r.UserName,
                Variable = r.ScreeningTemplateValue.ProjectDesignVariable.VariableName,
                Visit = r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ProjectDesignVisit.DisplayName +
                Convert.ToString(r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber),
                SiteCode = r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                PatientInitial = r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.Initial,
                ScreeningNo = r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber,
                RandomizationNo = r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber,
                StudyCode = ProjectCode,
                CollectionSource = r.ScreeningTemplateValue.ProjectDesignVariable.CollectionSource

            }).OrderByDescending(t => t.CreatedDate).ToList();

            #region Job Monitoring Save - Inprocess Status
            JobMonitoring jobMonitoring = new JobMonitoring();
            jobMonitoring.JobName = JobNameType.DataEntryAudit;
            jobMonitoring.JobDescription = filters.SelectedProject;
            jobMonitoring.JobType = filters.ExcelFormat ? JobTypeEnum.Excel : JobTypeEnum.Pdf;
            jobMonitoring.JobStatus = JobStatusType.InProcess;
            jobMonitoring.SubmittedBy = _jwtTokenAccesser.UserId;
            jobMonitoring.SubmittedTime = _jwtTokenAccesser.GetClientDate();
            _jobMonitoringRepository.Add(jobMonitoring);
            _context.Save();
            #endregion

            if (!filters.ExcelFormat)
            {

                #region PDF Report Design
                //Create a new PDF document
                using (PdfDocument doc = new PdfDocument())
                {
                    //Set the orientation
                    doc.PageSettings.Orientation = PdfPageOrientation.Landscape;

                    //Add a page
                    PdfPage page = doc.Pages.Add();

                    //Add header 
                    doc.Template.Top = AddHeader(doc, "Data Entry Audit Report");

                    //Add footer 
                    doc.Template.Bottom = AddFooter(doc);

                    //Create a PdfGrid
                    PdfGrid pdfGrid = new PdfGrid();

                    //Create a DataTable
                    DataTable dataTable = new DataTable();

                    //Add columns to the DataTable
                    dataTable.Columns.Add("STUDY CODE");
                    dataTable.Columns.Add("SITE CODE");
                    dataTable.Columns.Add("SCRNUM");
                    dataTable.Columns.Add("RANDNUM");
                    dataTable.Columns.Add("INITIAL");
                    dataTable.Columns.Add("VISIT");
                    dataTable.Columns.Add("Template");
                    dataTable.Columns.Add("Variable");
                    dataTable.Columns.Add("Old Value");
                    dataTable.Columns.Add("New Value");
                    dataTable.Columns.Add("User");
                    dataTable.Columns.Add("Role");
                    dataTable.Columns.Add("Reason");
                    dataTable.Columns.Add("Comment");
                    dataTable.Columns.Add("Note");
                    dataTable.Columns.Add("Created Date");
                    dataTable.Columns.Add("TimeZone");
                    dataTable.Columns.Add("IpAddress");

                    //Add rows to the DataTable
                    MainData.ForEach(d =>
                    {
                        dataTable.Rows.Add(new object[] { d.StudyCode, d.SiteCode, d.ScreeningNo, d.RandomizationNo, d.PatientInitial, d.Visit, d.Template, d.Variable, d.OldValue,
                    d.NewValue,d.User,d.Role,d.Reason,d.ReasonOth,d.Note,d.CreatedDate,d.TimeZone,d.IpAddress});
                    });

                    //Assign data source
                    pdfGrid.DataSource = dataTable;
                    pdfGrid.RepeatHeader = true;

                    //Create and customize string format  
                    PdfStringFormat format = new PdfStringFormat();
                    format.Alignment = PdfTextAlignment.Center;

                    //Specify the style for PdfGrid cell header
                    PdfGridCellStyle headerstyle = new PdfGridCellStyle();
                    headerstyle.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 6, PdfFontStyle.Bold);
                    headerstyle.StringFormat = format;
                    pdfGrid.Headers.ApplyStyle(headerstyle);

                    //Specify the style for PdfGrid cell content
                    PdfGridCellStyle cellstyle = new PdfGridCellStyle();
                    cellstyle.CellPadding = new PdfPaddings(2, 0, 1, 0);
                    pdfGrid.Rows.ApplyStyle(cellstyle);

                    //Draw the String.
                    pdfGrid.Style.Font = smallfont;

                    //Draw grid to the page of PDF document
                    pdfGrid.Draw(page, new PointF(10, 10));

                    string path = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.DataEntryAudit.ToString());
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var FileName = "audit_" + DateTime.Now.Ticks + ".pdf";
                    var FilePath = Path.Combine(path, FileName);

                    MemoryStream memoryStream = new MemoryStream();
                    doc.Save(memoryStream);
                    using (System.IO.FileStream fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Create))
                    {
                        memoryStream.WriteTo(fs);

                        #region Update Job Status
                        var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
                        string savepath = Path.Combine(documentUrl, FolderType.DataEntryAudit.ToString());
                        jobMonitoring.CompletedTime = _jwtTokenAccesser.GetClientDate();
                        jobMonitoring.JobStatus = JobStatusType.Completed;
                        jobMonitoring.FolderPath = savepath;
                        jobMonitoring.FolderName = FileName;
                        _jobMonitoringRepository.Update(jobMonitoring);
                        _context.Save();
                        #endregion

                        #region EmailSend
                        var user = _userRepository.Find(_jwtTokenAccesser.UserId);
                        var ProjectName = _context.Project.Find(filters.SelectedProject).ProjectCode + "-" + _context.Project.Find(filters.SelectedProject).ProjectName;
                        string pathofdoc = Path.Combine(savepath, FileName);
                        var linkOfDoc = "<a href='" + pathofdoc + "'>Click Here</a>";
                        _emailSenderRespository.SendDBDSGeneratedEMail(user.Email, _jwtTokenAccesser.UserName, ProjectName, linkOfDoc);
                        #endregion
                    }
                }

                #endregion PDF Report Design
            }
            else
            {
                #region Excel Report Design
                var repeatdata = new List<RepeatTemplateDto>();
                using (var workbook = new XLWorkbook())
                {
                    IXLWorksheet worksheet;
                    worksheet = workbook.Worksheets.Add();

                    worksheet.Rows(1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(1, 1).Value = "STUDY CODE";
                    worksheet.Cell(1, 2).Value = "SITE CODE";
                    worksheet.Cell(1, 3).Value = "SCRNUM";
                    worksheet.Cell(1, 4).Value = "RANDNUM";
                    worksheet.Cell(1, 5).Value = "INITIAL";
                    worksheet.Cell(1, 6).Value = "VISIT";
                    worksheet.Cell(1, 7).Value = "Template";
                    worksheet.Cell(1, 8).Value = "Variable";
                    worksheet.Cell(1, 9).Value = "Old Value";
                    worksheet.Cell(1, 10).Value = "New Value";
                    worksheet.Cell(1, 11).Value = "User";
                    worksheet.Cell(1, 12).Value = "Role";
                    worksheet.Cell(1, 13).Value = "Reason";
                    worksheet.Cell(1, 14).Value = "Comment";
                    worksheet.Cell(1, 15).Value = "Note";
                    worksheet.Cell(1, 16).Value = "Created Date";
                    worksheet.Cell(1, 17).Value = "TimeZone";
                    worksheet.Cell(1, 18).Value = "IpAddress";
                    var j = 3;

                    MainData.ForEach(d =>
                    {
                        worksheet.Row(j).Cell(1).SetValue(d.StudyCode);
                        worksheet.Row(j).Cell(2).SetValue(d.SiteCode);
                        worksheet.Row(j).Cell(3).SetValue(d.ScreeningNo);
                        worksheet.Row(j).Cell(4).SetValue(d.RandomizationNo);
                        worksheet.Row(j).Cell(5).SetValue(d.PatientInitial);
                        worksheet.Row(j).Cell(6).SetValue(d.Visit);
                        worksheet.Row(j).Cell(7).SetValue(d.Template);
                        worksheet.Row(j).Cell(8).SetValue(d.Variable);

                        #region old value
                        //if (d.CollectionSource == CollectionSources.DateTime && d.OldValue != "Default")
                        //{
                        //    DateTime dDate;
                        //    string variablevalueformat = d.OldValue;
                        //    var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variablevalueformat : "";
                        //    worksheet.Row(j).Cell(9).SetValue(dt);
                        //}
                        //else if (d.CollectionSource == CollectionSources.Date && d.OldValue != "Default")
                        //{
                        //    DateTime dDate;
                        //    string variablevalueformat = d.OldValue;
                        //    string dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.DateFormat, CultureInfo.InvariantCulture) : variablevalueformat : "";
                        //    worksheet.Row(j).Cell(9).SetValue(dt);
                        //}
                        //else if (d.CollectionSource == CollectionSources.Time && d.OldValue != "Default")
                        //{
                        //    string variablevalueformat = d.OldValue;
                        //    var dt = "";//!string.IsNullOrEmpty(variablevalueformat) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.TimeFormat, CultureInfo.InvariantCulture) : "";
                        //    worksheet.Row(j).Cell(9).SetValue(dt);
                        //}
                        //else
                        //{
                        //    worksheet.Row(j).Cell(9).SetValue(d.OldValue);
                        //}
                        #endregion old value
                        #region new value
                        //if (d.CollectionSource == CollectionSources.DateTime)
                        //{
                        //    DateTime dDate;
                        //    string variablevalueformat = d.NewValue;
                        //    var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variablevalueformat : "";
                        //    worksheet.Row(j).Cell(10).SetValue(dt);
                        //}
                        //else if (d.CollectionSource == CollectionSources.Date)
                        //{
                        //    DateTime dDate;
                        //    string variablevalueformat = d.NewValue;
                        //    string dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.DateFormat, CultureInfo.InvariantCulture) : variablevalueformat : "";
                        //    worksheet.Row(j).Cell(10).SetValue(dt);
                        //}

                        //else if (d.CollectionSource == CollectionSources.Time)
                        //{
                        //    DateTime dDate;
                        //    string variablevalueformat = d.NewValue;
                        //    var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.TimeFormat, CultureInfo.InvariantCulture) : variablevalueformat : "";
                        //    worksheet.Row(j).Cell(10).SetValue(dt);
                        //}
                        //else
                        //{
                        //    worksheet.Row(j).Cell(10).SetValue(d.NewValue);
                        //}
                        #endregion old value
                        worksheet.Row(j).Cell(9).SetValue(d.OldValue);
                        worksheet.Row(j).Cell(10).SetValue(d.NewValue);
                        worksheet.Row(j).Cell(11).SetValue(d.User);
                        worksheet.Row(j).Cell(12).SetValue(d.Role);
                        worksheet.Row(j).Cell(13).SetValue(d.Reason);
                        worksheet.Row(j).Cell(14).SetValue(d.ReasonOth);
                        worksheet.Row(j).Cell(15).SetValue(d.Note);
                        worksheet.Row(j).Cell(16).SetValue(d.CreatedDate);
                        worksheet.Row(j).Cell(17).SetValue(d.TimeZone);
                        worksheet.Row(j).Cell(18).SetValue(d.IpAddress);
                        j++;
                    });


                    string path = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.DataEntryAudit.ToString());
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();

                        stream.Position = 0;
                        var FileName = "audit_" + DateTime.Now.Ticks + ".xlsx";
                        var FilePath = Path.Combine(path, FileName);
                        workbook.SaveAs(FilePath);

                        #region Update Job Status
                        var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
                        string savepath = Path.Combine(documentUrl, FolderType.DataEntryAudit.ToString());
                        jobMonitoring.CompletedTime = _jwtTokenAccesser.GetClientDate();
                        jobMonitoring.JobStatus = JobStatusType.Completed;
                        jobMonitoring.FolderPath = savepath;
                        jobMonitoring.FolderName = FileName;
                        _jobMonitoringRepository.Update(jobMonitoring);
                        _context.Save();
                        #endregion


                        #region EmailSend
                        var user = _userRepository.Find(_jwtTokenAccesser.UserId);
                        var ProjectName = _context.Project.Find(filters.SelectedProject).ProjectCode + "-" + _context.Project.Find(filters.SelectedProject).ProjectName;
                        string pathofdoc = Path.Combine(savepath, FileName);
                        var linkOfDoc = "<a href='" + pathofdoc + "'>Click Here</a>";
                        _emailSenderRespository.SendDBDSGeneratedEMail(user.Email, _jwtTokenAccesser.UserName, ProjectName, linkOfDoc);
                        #endregion
                    }
                }
                #endregion
            }
        }

        public void Save(ScreeningTemplateValueAudit audit)
        {
            audit.IpAddress = _jwtTokenAccesser.IpAddress;
            audit.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            audit.UserName = _jwtTokenAccesser.UserName;
            audit.UserRole = _jwtTokenAccesser.RoleName;

            audit.CreatedDate = _jwtTokenAccesser.GetClientDate();

            Add(audit);
        }

        public PdfPageTemplateElement AddHeader(PdfDocument doc, string title)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create a page template
            PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 24);
            float doubleHeight = font.Height * 2;
            Color activeColor = Color.FromArgb(44, 71, 120);
            SizeF imageSize = new SizeF(110f, 35f);

            //Locating the logo on the right corner of the Drawing Surface
            PointF imageLocation = new PointF(doc.Pages[0].GetClientSize().Width - imageSize.Width - 20, 5);

            //PdfImage img = new PdfBitmap("../../Data/logo.png");

            ////Draw the image in the Header.
            //header.Graphics.DrawImage(img, imageLocation, imageSize);

            PdfSolidBrush brush = new PdfSolidBrush(activeColor);

            PdfPen pen = new PdfPen(Color.DarkBlue, 3f);
            font = new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold);

            //Set formattings for the text
            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Center;
            format.LineAlignment = PdfVerticalAlignment.Middle;

            //Draw title
            header.Graphics.DrawString(title, font, brush, new RectangleF(0, 0, header.Width, header.Height), format);
            brush = new PdfSolidBrush(Color.Gray);
            font = new PdfStandardFont(PdfFontFamily.Helvetica, 6, PdfFontStyle.Bold);

            format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.LineAlignment = PdfVerticalAlignment.Bottom;

            ////Draw description
            //header.Graphics.DrawString(DateTime.Now.ToString(), font, brush, new RectangleF(0, 0, header.Width, header.Height - 8), format);

            //Draw some lines in the header
            pen = new PdfPen(Color.DarkBlue, 0.7f);
            header.Graphics.DrawLine(pen, 0, 0, header.Width, 0);
            pen = new PdfPen(Color.DarkBlue, 2f);
            header.Graphics.DrawLine(pen, 0, 03, header.Width + 3, 03);
            pen = new PdfPen(Color.DarkBlue, 2f);
            header.Graphics.DrawLine(pen, 0, header.Height - 3, header.Width, header.Height - 3);
            header.Graphics.DrawLine(pen, 0, header.Height, header.Width, header.Height);

            return header;
        }

        public PdfPageTemplateElement AddFooter(PdfDocument doc)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 10);
            PdfPageTemplateElement footer = new PdfPageTemplateElement(rect);
            PdfFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 8, PdfFontStyle.Bold);
            PdfSolidBrush brush = new PdfSolidBrush(Color.Black);

            PdfPageNumberField pageNumber = new PdfPageNumberField(font, brush);
            PdfPageCountField count = new PdfPageCountField(font, brush);

            PdfCompositeField compositeField = new PdfCompositeField(font, brush, "Page {0} of {1}", pageNumber, count);
            compositeField.Bounds = footer.Bounds;
            compositeField.Draw(footer.Graphics, new PointF(footer.Width - 60, footer.Height - 10));

            PdfCompositeField createdBy = new PdfCompositeField(font, brush, "Created By : " + _jwtTokenAccesser.UserName + "(" + _jwtTokenAccesser.RoleName + ")");
            createdBy.Bounds = footer.Bounds;
            createdBy.Draw(footer.Graphics, new PointF(1, footer.Height - 10));

            PdfCompositeField createdDate = new PdfCompositeField(font, brush, "Created On : " + DateTime.Now.ToString());
            createdDate.Bounds = footer.Bounds;
            createdDate.Draw(footer.Graphics, new PointF((footer.Width / 2) - 20, footer.Height - 10));

            PdfPen pen = new PdfPen(Color.Black, 1.0f);
            footer.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);

            return footer;
        }
    }
}