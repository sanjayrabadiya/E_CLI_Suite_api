using AutoMapper;
using ClosedXML.Excel;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Data.Entities.Report;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Reports;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public class EconsentReviewDetailsAuditRepository : GenericRespository<EconsentReviewDetailsAudit>, IEconsentReviewDetailsAuditRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly PdfFont smallfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 6);
        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        public EconsentReviewDetailsAuditRepository(IGSCContext context,
                                    IJwtTokenAccesser jwtTokenAccesser,
                                    IMapper mapper,
                                    IUnitOfWork uow,
                                    IUploadSettingRepository uploadSettingRepository,
                                    IJobMonitoringRepository jobMonitoringRepository) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _uow = uow;
            _uploadSettingRepository = uploadSettingRepository;
            _jobMonitoringRepository = jobMonitoringRepository;
        }

        public void GenerateICFDetailReport(EconsentReviewDetailsAuditParameterDto details)
        {

            var resultdetails = All.Where(y => y.EconsentReviewDetails.EconsentSetup.ProjectId == details.ParentProjectId
             && (details.ProjectId > 0 ? y.EconsentReviewDetails.Randomization.ProjectId == details.ProjectId : true)
             && (details.DocumentId > 0 ? y.EconsentReviewDetails.EconsentSetup.Id == details.DocumentId : true)
             && (details.ActionId > 0 ? y.Activity == details.ActionId : true)
             && (details.PatientStatusId > 0 ? y.PateientStatus == details.PatientStatusId : true)
             && (details.SubjectIds.Count() > 0 ? details.SubjectIds.Select(x => x.Id).Contains(y.EconsentReviewDetails.RandomizationId) : true)
             ).Select(x => new EconsentReviewDetailsAuditGridDto
                {
                    Key = x.EconsentReviewDetails.Randomization.Id,
                    StudyCode = x.EconsentReviewDetails.EconsentSetup.Project.ProjectCode,
                    SiteCode = x.EconsentReviewDetails.Randomization.Project.ProjectCode,
                    DocumentName = x.EconsentReviewDetails.EconsentSetup.DocumentName,
                    ScreeningNumber = x.EconsentReviewDetails.Randomization.ScreeningNumber,
                    RandomizationNumber = x.EconsentReviewDetails.Randomization.RandomizationNumber,
                    Initial = x.EconsentReviewDetails.Randomization.Initial,
                    Version = x.EconsentReviewDetails.EconsentSetup.Version,
                    LanguageName = x.EconsentReviewDetails.EconsentSetup.Language.LanguageName,
                    Activity = x.Activity.GetDescription(),
                    PatientStatus = x.PateientStatus.GetDescription(),
                    CreatedByUser = x.CreatedByUser.UserName,
                    CreatedDate = x.CreatedDate
                }).ToList();
            JobMonitoring jobMonitoring = new JobMonitoring();
            jobMonitoring.JobName = JobNameType.ICFDetailReport;
            jobMonitoring.JobDescription = details.ParentProjectId;
            jobMonitoring.JobType = details.isExcel ? JobTypeEnum.Excel : JobTypeEnum.Pdf;
            jobMonitoring.JobStatus = JobStatusType.InProcess;
            jobMonitoring.SubmittedBy = _jwtTokenAccesser.UserId;
            jobMonitoring.SubmittedTime = _jwtTokenAccesser.GetClientDate();
            _jobMonitoringRepository.Add(jobMonitoring);
            _context.Save();
            if (!details.isExcel)
                GeneratePdfReport(resultdetails, jobMonitoring);
            else
                GenerateExcelReport(resultdetails, jobMonitoring);
        }
        private void GeneratePdfReport(List<EconsentReviewDetailsAuditGridDto> data, JobMonitoring jobMonitoring)
        {
            using (PdfDocument doc = new PdfDocument())
            {
                //Set the orientation
                doc.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Add a page
                PdfPage page = doc.Pages.Add();
                //Add header 
                doc.Template.Top = AddHeader(doc, "ICF Detail Report");
                //Add footer 
                doc.Template.Bottom = AddFooter(doc);
                //Create a PdfGrid
                PdfGrid pdfGrid = new PdfGrid();

                //Create a DataTable
                DataTable dataTable = new DataTable();

                //Add columns to the DataTable
                dataTable.Columns.Add("Key No");
                dataTable.Columns.Add("Study Code");
                dataTable.Columns.Add("Site Code");
                dataTable.Columns.Add("Screen No");
                dataTable.Columns.Add("Randomization No");
                dataTable.Columns.Add("Initial");
                dataTable.Columns.Add("Document Name");
                dataTable.Columns.Add("Version");
                dataTable.Columns.Add("Language");
                dataTable.Columns.Add("Activity");
                dataTable.Columns.Add("Patient Status");
                dataTable.Columns.Add("Done By");
                dataTable.Columns.Add("Done Date");
                //Add rows to the DataTable
                data.ForEach(d =>
                {
                    dataTable.Rows.Add(new object[] {
                        d.Key,
                        d.StudyCode,
                        d.SiteCode,
                        d.ScreeningNumber,
                        d.RandomizationNumber,
                        d.Initial,
                        d.DocumentName,
                        d.Version,
                        d.LanguageName,
                        d.Activity,
                        d.PatientStatus,
                        d.CreatedByUser,
                        d.CreatedDate});
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

                string path = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ICFDetailReport.ToString());
                //string path = @"D:\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var FileName = "IcfdetailReport_" + DateTime.Now.Ticks + ".pdf";
                var FilePath = Path.Combine(path, FileName);

                MemoryStream memoryStream = new MemoryStream();
                doc.Save(memoryStream);
                using (System.IO.FileStream fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Create))
                {
                    memoryStream.WriteTo(fs);

                    #region Update Job Status
                    var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
                    string savepath = Path.Combine(documentUrl, FolderType.ICFDetailReport.ToString());
                    jobMonitoring.CompletedTime = _jwtTokenAccesser.GetClientDate();
                    jobMonitoring.JobStatus = JobStatusType.Completed;
                    jobMonitoring.FolderPath = savepath;
                    jobMonitoring.FolderName = FileName;
                    _jobMonitoringRepository.Update(jobMonitoring);
                    _context.Save();
                    #endregion

                    //#region EmailSend
                    //var user = _userRepository.Find(_jwtTokenAccesser.UserId);
                    //var ProjectName = _context.Project.Find(filters.SelectedProject).ProjectCode + "-" + _context.Project.Find(filters.SelectedProject).ProjectName;
                    //string pathofdoc = Path.Combine(savepath, FileName);
                    //var linkOfDoc = "<a href='" + pathofdoc + "'>Click Here</a>";
                    //_emailSenderRespository.SendDBDSGeneratedEMail(user.Email, _jwtTokenAccesser.UserName, ProjectName, linkOfDoc);
                    //#endregion
                }
            }

        }

        private void GenerateExcelReport(List<EconsentReviewDetailsAuditGridDto> data, JobMonitoring jobMonitoring)
        {
            //var repeatdata = new List<RepeatTemplateDto>();
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet;
                worksheet = workbook.Worksheets.Add();

                worksheet.Rows(1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "Key No";
                worksheet.Cell(1, 2).Value = "Study Code";
                worksheet.Cell(1, 3).Value = "Site Code";
                worksheet.Cell(1, 4).Value = "Screen No";
                worksheet.Cell(1, 5).Value = "Randomization No";
                worksheet.Cell(1, 6).Value = "Initial";
                worksheet.Cell(1, 7).Value = "Document Name";
                worksheet.Cell(1, 8).Value = "Version";
                worksheet.Cell(1, 9).Value = "Language";
                worksheet.Cell(1, 10).Value = "Activity";
                worksheet.Cell(1, 11).Value = "Patient Status";
                worksheet.Cell(1, 12).Value = "Done By";
                worksheet.Cell(1, 13).Value = "Done Date";
                var j = 3;

                data.ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.SiteCode);
                    worksheet.Row(j).Cell(4).SetValue(d.ScreeningNumber);
                    worksheet.Row(j).Cell(5).SetValue(d.RandomizationNumber);
                    worksheet.Row(j).Cell(6).SetValue(d.Initial);
                    worksheet.Row(j).Cell(7).SetValue(d.DocumentName);
                    worksheet.Row(j).Cell(8).SetValue(d.Version);
                    worksheet.Row(j).Cell(9).SetValue(d.LanguageName);
                    worksheet.Row(j).Cell(10).SetValue(d.Activity);
                    worksheet.Row(j).Cell(11).SetValue(d.PatientStatus);
                    worksheet.Row(j).Cell(12).SetValue(d.CreatedByUser);
                    worksheet.Row(j).Cell(13).SetValue(d.CreatedDate);
                    j++;
                });


                string path = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ICFDetailReport.ToString());
                //string path = @"D:\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    stream.Position = 0;
                    var FileName = "IcfdetailReport__" + DateTime.Now.Ticks + ".xlsx";
                    var FilePath = Path.Combine(path, FileName);
                    workbook.SaveAs(FilePath);

                    #region Update Job Status
                    var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
                    string savepath = Path.Combine(documentUrl, FolderType.ICFDetailReport.ToString());                   
                    jobMonitoring.CompletedTime = _jwtTokenAccesser.GetClientDate();
                    jobMonitoring.JobStatus = JobStatusType.Completed;
                    jobMonitoring.FolderPath = savepath;
                    jobMonitoring.FolderName = FileName;
                    _jobMonitoringRepository.Update(jobMonitoring);
                    _context.Save();
                    #endregion

                    //#region EmailSend
                    //var user = _userRepository.Find(_jwtTokenAccesser.UserId);
                    //var ProjectName = _context.Project.Find(filters.SelectedProject).ProjectCode + "-" + _context.Project.Find(filters.SelectedProject).ProjectName;
                    //string pathofdoc = Path.Combine(savepath, FileName);
                    //var linkOfDoc = "<a href='" + pathofdoc + "'>Click Here</a>";
                    //_emailSenderRespository.SendDBDSGeneratedEMail(user.Email, _jwtTokenAccesser.UserName, ProjectName, linkOfDoc);
                    // #endregion
                }
            }
        }

        private PdfPageTemplateElement AddHeader(PdfDocument doc, string title)
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

        private PdfPageTemplateElement AddFooter(PdfDocument doc)
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
