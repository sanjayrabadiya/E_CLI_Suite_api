using BoldReports.Web;
using BoldReports.Writer;
using GSC.Data.Dto.Common;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Report.Pdf;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Report;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Report.Common;
using GSC.Respository.Client;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Etmf;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace GSC.Report
{
    public class ReportSyncfusion : IReportSyncfusion
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IReportBaseRepository _reportBaseRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IGSCContext _context;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;


        private readonly PdfFont watermarkerfornt = new PdfStandardFont(PdfFontFamily.TimesRoman, 120, PdfFontStyle.Bold);
        private readonly PdfFont largeheaderfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);
        private readonly PdfFont headerfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 14, PdfFontStyle.Bold);
        private readonly PdfFont categoryfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Bold);
        private readonly PdfFont smallfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 8);
        private readonly PdfFont regularfont;
        private readonly PdfFont italicfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 10, PdfFontStyle.Italic);
        private readonly Stream fontStream;
        private readonly ISyncConfigurationMasterRepository _syncConfigurationMasterRepository;

        private PdfDocument document = null;
        private PdfLayoutResult tocresult = null;
        Dictionary<PdfPageBase, int> pages = new Dictionary<PdfPageBase, int>();
        private List<TocIndexCreate> _pagenumberset = new List<TocIndexCreate>();

        public ReportSyncfusion(IHostingEnvironment hostingEnvironment,
        IUploadSettingRepository uploadSettingRepository, IReportBaseRepository reportBaseRepository, ICompanyRepository companyRepository,
        IClientRepository clientRepository, IGSCContext context, IAppSettingRepository appSettingRepository, IJwtTokenAccesser jwtTokenAccesser,
        IUserRepository userRepository, IEmailSenderRespository emailSenderRespository, ISyncConfigurationMasterRepository syncConfigurationMasterRepository
        )
        {
            _hostingEnvironment = hostingEnvironment;
            _uploadSettingRepository = uploadSettingRepository;
            _reportBaseRepository = reportBaseRepository;
            _companyRepository = companyRepository;
            _clientRepository = clientRepository;
            _context = context;
            _appSettingRepository = appSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRepository = userRepository;
            _emailSenderRespository = emailSenderRespository;
            fontStream = FilePathConvert();
            regularfont = new PdfTrueTypeFont(fontStream, 12);
            _syncConfigurationMasterRepository = syncConfigurationMasterRepository;
        }

        private Stream FilePathConvert()
        {
            string path = _hostingEnvironment.WebRootPath + "/fonts/times.ttf";
            byte[] file = File.ReadAllBytes(path);
            Stream stream = new MemoryStream(file);
            return stream;
        }
        //pdf header
        private PdfPageTemplateElement AddHeader(PdfDocument doc, string studyName, bool isClientLogo, bool isCompanyLogo, int ClientId)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 70);
            PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 24);
            Color activeColor = Color.FromArgb(44, 71, 120);
            SizeF imageSize = new SizeF(50f, 50f);

            var imagePath = _uploadSettingRepository.GetImagePath();
            var companydetail = _companyRepository.All.Select(x => new { x.Logo, x.CompanyName }).FirstOrDefault();
            if (isCompanyLogo)
            {
                if (File.Exists($"{imagePath}/{companydetail.Logo}") && !String.IsNullOrEmpty(companydetail.Logo))
                {
                    FileStream logoinputstream = new FileStream($"{imagePath}/{companydetail.Logo}", FileMode.Open, FileAccess.Read);
                    PdfImage img = new PdfBitmap(logoinputstream);
                    var companylogo = new PointF(20, 0);
                    header.Graphics.DrawImage(img, companylogo, imageSize);
                }
            }
            if (isClientLogo)
            {
                var clientlogopath = _clientRepository.All.Where(x => x.Id == ClientId).Select(x => x.Logo).FirstOrDefault();
                if (File.Exists($"{imagePath}/{clientlogopath}") && !String.IsNullOrEmpty(clientlogopath))
                {
                    FileStream logoinputstream = new FileStream($"{imagePath}/{clientlogopath}", FileMode.Open, FileAccess.Read);
                    PdfImage img = new PdfBitmap(logoinputstream);
                    var imageLocation = new PointF(doc.Pages[0].GetClientSize().Width - imageSize.Width - 20, 0);
                    header.Graphics.DrawImage(img, imageLocation, imageSize);
                }
            }

            PdfSolidBrush brush = new PdfSolidBrush(activeColor);
            PdfPen pen = new PdfPen(Color.DarkBlue, 3f);
            font = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);

            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Center;
            format.LineAlignment = PdfVerticalAlignment.Top;


            header.Graphics.DrawString($"{companydetail.CompanyName}", font, brush, new RectangleF(0, 0, header.Width, header.Height), format);
            brush = new PdfSolidBrush(Color.Gray);
            font = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);

            header.Graphics.DrawString("CASE REPORT FORM", font, brush, new RectangleF(0, 20, header.Width, header.Height), format);
            brush = new PdfSolidBrush(Color.Gray);
            font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Bold);
            header.Graphics.DrawString($"Study Code :- {studyName}", font, brush, new RectangleF(0, 40, header.Width, header.Height), format);


            format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.LineAlignment = PdfVerticalAlignment.Bottom;

            pen = new PdfPen(Color.Black, 2f);
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
            //string prientedby = "Printed By : " + _jwtTokenAccesser.UserName + " (" + DateTime.Now.ToString("dd-MM-yyyy h:mm tt") + ")";
            string prientedby = "Printed By : " + _jwtTokenAccesser.UserName + " (" + _jwtTokenAccesser.GetClientDate().ToString("dd-MM-yyyy h:mm tt") + ")";

            PdfCompositeField compositeFieldprintedby = new PdfCompositeField(font, brush, prientedby);
            compositeFieldprintedby.Bounds = footer.Bounds;

            compositeField.Draw(footer.Graphics, new PointF(footer.Width - 70, footer.Height - 10));

            compositeFieldprintedby.Draw(footer.Graphics, new PointF(footer.Width / 3, footer.Height - 10));

            PdfPen pen = new PdfPen(Color.Black, 1.0f);
            footer.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);

            return footer;
        }

        private PdfLayoutResult AddString(string note, PdfPage page, RectangleF position, PdfBrush brush, PdfFont font, PdfLayoutFormat pdfLayoutFormat)
        {
            //font.SetTextEncoding(Encoding.GetEncoding("Windows-1250"))
            //PdfTrueTypeFont fonts = new PdfTrueTypeFont(new Font("Microsoft Sans Serif", 14), true);

            PdfTextElement richTextElement = new PdfTextElement(String.IsNullOrEmpty(note) ? " " : note, font, brush);
            //Draws String       
            PdfStringFormat stringFormat = new PdfStringFormat();
            stringFormat.MeasureTrailingSpaces = true;
            stringFormat.WordWrap = PdfWordWrapType.Word;
            richTextElement.StringFormat = stringFormat;

            PdfLayoutResult result = richTextElement.Draw(page, position, pdfLayoutFormat);
            return result;
        }

        private PdfLayoutResult AddStringCategory(string note, PdfPage page, RectangleF position, PdfBrush brush, PdfFont font, PdfLayoutFormat pdfLayoutFormat)
        {
            

            PdfTextElement richTextElement = new PdfTextElement(String.IsNullOrEmpty(note) ? " " : note, font, brush);
            //Draws String       
            PdfStringFormat stringFormat = new PdfStringFormat();
            stringFormat.MeasureTrailingSpaces = true;
            stringFormat.WordWrap = PdfWordWrapType.Word;
            stringFormat.Alignment = PdfTextAlignment.Center;
            richTextElement.StringFormat = stringFormat;
            PointF point = new PointF(0, 0);
            page.Graphics.DrawString("", font, PdfBrushes.Black, point);
            PdfSolidBrush brush1 = new PdfSolidBrush(Color.LightGray);
            //RectangleF bounds = new RectangleF(0, 0, 300, 300);
            page.Graphics.Save();
            page.Graphics.SetTransparency(1, 1, PdfBlendMode.Multiply);

            //Draw the rectangle on the PDF document
            page.Graphics.DrawRectangle(brush1, position);

            PdfLayoutResult result = richTextElement.Draw(page, position, pdfLayoutFormat);
            return result;
        }

        public PdfBookmark AddBookmark(PdfLayoutResult sectionpage, string title, bool isVisit)
        {
            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            layoutFormat.Layout = PdfLayoutType.Paginate;
            layoutFormat.Break = PdfLayoutBreakType.FitElement;

            PdfBookmark bookmarks = document.Bookmarks.Add(title);
            bookmarks.Destination = new PdfDestination(sectionpage.Page);
            bookmarks.Destination.Location = new PointF(0, sectionpage.Bounds.Y);
            //AddTableOfcontents(sectionpage, title, isVisit);
            // Adding bookmark with named destination
            PdfNamedDestination namedDestination = new PdfNamedDestination(title);
            namedDestination.Destination = new PdfDestination(sectionpage.Page, new PointF(0, sectionpage.Bounds.Y));
            namedDestination.Destination.Mode = PdfDestinationMode.FitToPage;
            document.NamedDestinationCollection.Add(namedDestination);
            bookmarks.NamedDestination = namedDestination;
            return bookmarks;
        }

        public PdfBookmark AddSection(PdfBookmark bookmark, PdfLayoutResult page, string title)
        {
            //PdfGraphics graphics = page.Graphics;
            //Add bookmark in PDF document
            PdfBookmark bookmarks = bookmark.Add(title);

            //Draw the content in the PDF page
            //graphics.DrawString(title, regularfont, PdfBrushes.Black, new PointF(point.X, point.Y));

            bookmarks.Destination = new PdfDestination(page.Page, new PointF(0, page.Bounds.Y));
            //bookmarks.Destination = new PdfDestination(page.Page);
            bookmarks.Destination.Location = new PointF(0, page.Bounds.Y);

            return bookmarks;
        }

        public void AddTableOfcontents(PdfLayoutResult page, string title, bool isVisit)
        {
            PdfTextElement element;
            if (isVisit)
                element = new PdfTextElement($"{title}", headerfont, PdfBrushes.Black);
            else
                element = new PdfTextElement($"{title}", regularfont, PdfBrushes.Black);
            //Set layout format for pagination of TOC
            PdfLayoutFormat format = new PdfLayoutFormat();
            format.Break = PdfLayoutBreakType.FitPage;
            format.Layout = PdfLayoutType.Paginate;
            tocresult = element.Draw(tocresult.Page, new PointF(isVisit ? 0 : 10, tocresult.Bounds.Y + 20), format);
            //Draw page number in TOC
            PdfTextElement pageNumber = new PdfTextElement(document.Pages.IndexOf(page.Page).ToString(), regularfont, PdfBrushes.Black);
            pageNumber.Draw(tocresult.Page, new PointF(tocresult.Page.Graphics.ClientSize.Width - 40, tocresult.Bounds.Y));

            PdfDocumentLinkAnnotation documentLinkAnnotation = new PdfDocumentLinkAnnotation(tocresult.Bounds);
            documentLinkAnnotation.AnnotationFlags = PdfAnnotationFlags.NoRotate;
            documentLinkAnnotation.Text = title;
            documentLinkAnnotation.Color = Color.Transparent;
            //Sets the destination
            documentLinkAnnotation.Destination = new PdfDestination(page.Page);
            documentLinkAnnotation.Destination.Location = new PointF(0, tocresult.Bounds.Y);
            //Adds this annotation to a new page
            tocresult.Page.Annotations.Add(documentLinkAnnotation);
        }
        private PdfPageTemplateElement VisitTemplateHeader(PdfDocument doc, string projectcode, string vistName, string screeningNO, string subjectNo, string Initial, bool Isscreeningno, bool isSubjectNo, bool IsInitial, bool isSiteCode)
        {
            if (vistName.Length < 45)
            {
                RectangleF rect = new RectangleF(0, 80, doc.Pages[0].GetClientSize().Width, 120);
                PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
                PdfStringFormat stringFormat = new PdfStringFormat();
                stringFormat.MeasureTrailingSpaces = true;
                stringFormat.WordWrap = PdfWordWrapType.Character;

                //Draw title
                header.Graphics.DrawString($"Visit : {vistName}", headerfont, PdfBrushes.Black, new RectangleF(0, 80, 340, header.Height), stringFormat);
                if (Isscreeningno)
                    header.Graphics.DrawString($"Screening No.: {screeningNO}", headerfont, PdfBrushes.Black, new RectangleF(0, 100, 340, header.Height), stringFormat);
                if (isSubjectNo)
                    header.Graphics.DrawString($"Subject No : {subjectNo}", headerfont, PdfBrushes.Black, new RectangleF(350, 80, header.Width, header.Height), stringFormat);
                if (IsInitial)
                    header.Graphics.DrawString($"Initial : {Initial}", headerfont, PdfBrushes.Black, new RectangleF(350, 100, header.Width, header.Height), stringFormat);
                if (isSiteCode)
                    header.Graphics.DrawString($"SiteCode : {projectcode}", headerfont, PdfBrushes.Black, new RectangleF(0, 120, header.Width, header.Height), stringFormat);
                return header;
            }
            else
            {
                RectangleF rect = new RectangleF(0, 90, doc.Pages[0].GetClientSize().Width, 150);
                PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
                PdfStringFormat stringFormat = new PdfStringFormat();
                stringFormat.MeasureTrailingSpaces = true;
                stringFormat.WordWrap = PdfWordWrapType.Word;

                //Draw title
                header.Graphics.DrawString($"Visit : {vistName}", headerfont, PdfBrushes.Black, new RectangleF(0, 80, 340, header.Height), stringFormat);
                if (Isscreeningno)
                    header.Graphics.DrawString($"Screening No.: {screeningNO}", headerfont, PdfBrushes.Black, new RectangleF(0, 110, 340, header.Height), stringFormat);
                if (isSubjectNo)
                    header.Graphics.DrawString($"Subject No : {subjectNo}", headerfont, PdfBrushes.Black, new RectangleF(350, 80, header.Width, header.Height), stringFormat);
                if (IsInitial)
                    header.Graphics.DrawString($"Initial : {Initial}", headerfont, PdfBrushes.Black, new RectangleF(350, 110, header.Width, header.Height), stringFormat);
                if (isSiteCode)
                    header.Graphics.DrawString($"SiteCode : {projectcode}", headerfont, PdfBrushes.Black, new RectangleF(0, 120, header.Width, header.Height), stringFormat);
                return header;
            }
        }

        public FileStreamResult GetProjectDesign(ReportSettingNew reportSetting)
        {
            //var projectdetails = _projectDesignRepository.FindByInclude(i => i.ProjectId == reportSetting.ProjectId, i => i.Project).SingleOrDefault();
            //var projectDesignvisit = _projectDesignVisitRepository.GetVisitsByProjectDesignId(projectdetails.Id);

            var projectDetails = _reportBaseRepository.GetBlankPdfData(reportSetting);

            document = new PdfDocument();
            document.PageSettings.Margins.Top = Convert.ToInt32(reportSetting.TopMargin * 100);
            document.PageSettings.Margins.Bottom = Convert.ToInt32(reportSetting.BottomMargin * 100);
            document.PageSettings.Margins.Left = Convert.ToInt32(reportSetting.LeftMargin * 100);
            document.PageSettings.Margins.Right = Convert.ToInt32(reportSetting.RightMargin * 100);
            foreach (var item in projectDetails)
            {
                foreach (var designperiod in item.Period)
                {
                    DesignVisitReport(designperiod.Visit, reportSetting, item);
                }
            }
            if (reportSetting.PdfType == 1)
            {
                foreach (PdfPage page in document.Pages)
                {
                    // water marker                 
                    PdfGraphics graphics = page.Graphics;
                    //Draw watermark text
                    PdfGraphicsState state = graphics.Save();
                    graphics.SetTransparency(0.25f);
                    graphics.RotateTransform(-40);
                    graphics.DrawString("Draft", watermarkerfornt, PdfPens.LightBlue, PdfBrushes.LightBlue, new PointF(-100, 300));
                    graphics.Restore();
                }
            }

            PdfBookmarkBase bookmarks = document.Bookmarks;
            //Iterates through bookmarks
            foreach (PdfBookmark bookmark in bookmarks)
            {
                IndexCreate(bookmark, false);
                foreach (PdfBookmark subbookmark in bookmark)
                {
                    IndexCreate(subbookmark, true);
                }
            }
            SetPageNumber();
            MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Position = 0;
            FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/pdf");
            fileStreamResult.FileDownloadName = "blankreport.pdf";
            return fileStreamResult;

        }

        private void IndexCreate(PdfBookmark bookmark, bool isSubSection)
        {
            PdfLayoutFormat layoutformat = new PdfLayoutFormat();
            layoutformat.Break = PdfLayoutBreakType.FitPage;
            layoutformat.Layout = PdfLayoutType.Paginate;
            PdfPageBase page = bookmark.Destination.Page;

            PdfDocumentLinkAnnotation documentLinkAnnotation = new PdfDocumentLinkAnnotation(new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height));
            documentLinkAnnotation.AnnotationFlags = PdfAnnotationFlags.NoRotate;
            documentLinkAnnotation.Text = bookmark.Title;
            documentLinkAnnotation.Color = Color.Transparent;
            //Sets the destination
            documentLinkAnnotation.Destination = new PdfDestination(bookmark.Destination.Page);
            documentLinkAnnotation.Destination.Location = new PointF(tocresult.Bounds.X, tocresult.Bounds.Y + 20);
            //Adds this annotation to a new page
            tocresult.Page.Annotations.Add(documentLinkAnnotation);
            if (isSubSection)
            {
                PdfTextElement element = new PdfTextElement($"{bookmark.Title}", regularfont, PdfBrushes.Black);
                tocresult = element.Draw(tocresult.Page, new PointF(10, tocresult.Bounds.Y + 20), layoutformat);
                _pagenumberset.Add(new TocIndexCreate { TocPage = tocresult.Page, Point = new PointF(tocresult.Page.Graphics.ClientSize.Width - 40, tocresult.Bounds.Y), bookmark = bookmark });
            }
            else
            {
                PdfTextElement element = new PdfTextElement($"{bookmark.Title}", headerfont, PdfBrushes.Black);
                tocresult = element.Draw(tocresult.Page, new PointF(0, tocresult.Bounds.Y + 20), layoutformat);
                _pagenumberset.Add(new TocIndexCreate { TocPage = tocresult.Page, Point = new PointF(tocresult.Page.Graphics.ClientSize.Width - 40, tocresult.Bounds.Y), bookmark = bookmark });
            }
        }

        private void SetPageNumber()
        {

            for (int i = 0; i < document.Pages.Count; i++)
            {
                PdfPageBase page = document.Pages[i] as PdfPageBase;
                //Add the page and index to dictionary 
                pages.Add(page, i);
            }

            for (int i = 0; i < _pagenumberset.Count; i++)
            {
                PdfPageBase page = _pagenumberset[i].bookmark.Destination.Page;
                if (pages.ContainsKey(page))
                {
                    int pagenumber = pages[page];
                    pagenumber++;
                    PdfTextElement pageNumber = new PdfTextElement(pagenumber.ToString(), regularfont, PdfBrushes.Black);
                    pageNumber.Draw(_pagenumberset[i].TocPage, _pagenumberset[i].Point);
                }
            }

        }


        //Report Generate
        public string DossierPdfReportGenerate(ReportSettingNew reportSetting, JobMonitoring jobMonitoring)
        {

            var projectDetails = new List<DossierReportDto>();
            if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
            {
                projectDetails = _reportBaseRepository.GetBlankPdfData(reportSetting);
            }
            else
            {
                projectDetails = _reportBaseRepository.GetDataPdfReport(reportSetting);
                if (projectDetails.Count == 0)
                    return "Data Entery is pending.";
            }


            //var projectdetails = _projectDesignRepository.FindByInclude(i => i.ProjectId == reportSetting.ProjectId && i.Project.IsTestSite == false, i => i.Project).SingleOrDefault();
            //var projectDesignvisit = _projectDesignVisitRepository.GetVisitsByProjectDesignId(projectdetails.Id);
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            FileSaveInfo fileInfo = new FileSaveInfo();
            var base_URL = _uploadSettingRepository.All.OrderByDescending(x => x.Id).FirstOrDefault().DocumentPath;

            var parent = _context.Project.Where(x => x.Id == reportSetting.ProjectId).FirstOrDefault().ProjectCode;
            fileInfo.ParentFolderName = parent + "_" + DateTime.Now.Ticks;

            string ParentProctCode = projectDetails.FirstOrDefault().ProjectDetails.ProjectCode;
            string ParentProjectName = projectDetails.FirstOrDefault().ProjectDetails.ProjectName;

            foreach (var item in projectDetails)
            {
                document = new PdfDocument();
                document.PageSettings.Margins.Top = Convert.ToInt32(reportSetting.TopMargin * 100);
                document.PageSettings.Margins.Bottom = Convert.ToInt32(reportSetting.BottomMargin * 100);
                document.PageSettings.Margins.Left = Convert.ToInt32(reportSetting.LeftMargin * 100);
                document.PageSettings.Margins.Right = Convert.ToInt32(reportSetting.RightMargin * 100);

                foreach (var designperiod in item.Period)
                {
                    DesignVisitReport(designperiod.Visit, reportSetting, item);
                }


                if (reportSetting.PdfType == 1)
                {
                    foreach (PdfPage page in document.Pages)
                    {
                        // water marker                 
                        PdfGraphics graphics = page.Graphics;
                        //Draw watermark text
                        PdfGraphicsState state = graphics.Save();
                        graphics.SetTransparency(0.25f);
                        graphics.RotateTransform(-40);
                        graphics.DrawString("Draft", watermarkerfornt, PdfPens.LightBlue, PdfBrushes.LightBlue, new PointF(-100, 300));
                        graphics.Restore();
                    }
                }
                PdfBookmarkBase bookmarks = document.Bookmarks;
                foreach (PdfBookmark bookmark in bookmarks)
                {
                    IndexCreate(bookmark, false);
                    foreach (PdfBookmark subbookmark in bookmark)
                    {
                        IndexCreate(subbookmark, true);
                    }
                }
                SetPageNumber();
                MemoryStream memoryStream = new MemoryStream();
                document.Save(memoryStream);

                fileInfo.Base_URL = base_URL;
                fileInfo.ModuleName = Enum.GetName(typeof(JobNameType), jobMonitoring.JobName);
                fileInfo.FolderType = Enum.GetName(typeof(DossierPdfStatus), jobMonitoring.JobDetails);

                //projectDetails.FirstOrDefault().ProjectDetails.ProjectCode + "-" + projectDetails.FirstOrDefault().ProjectDetails.ProjectName;
                if (reportSetting.IsSync)
                {
                    string filenName = reportSetting.PdfStatus == DossierPdfStatus.Blank ? fileInfo.ParentFolderName.Replace("/", "") + ".pdf" : item.Initial.Replace("/", "") + ".pdf";
                    SyncFile(filenName, reportSetting, memoryStream);
                }
                else
                {
                    SaveFile(reportSetting, fileInfo, memoryStream, item, ParentProctCode);
                }
            }

            if (!reportSetting.IsSync)
                UpdateJobStatus(jobMonitoring, fileInfo, ParentProctCode, ParentProjectName, documentUrl);

            return "";
        }

        private void DesignVisitReport(List<ProjectDesignVisitList> designvisit, ReportSettingNew reportSetting, DossierReportDto details)
        {
            PdfSection SectionTOC = document.Sections.Add();
            PdfPage pageTOC = SectionTOC.Pages.Add();

            document.Template.Top = AddHeader(document, details.ProjectDetails.ProjectCode, Convert.ToBoolean(reportSetting.IsClientLogo), Convert.ToBoolean(reportSetting.IsCompanyLogo), details.ProjectDetails.ClientId);
            document.Template.Bottom = AddFooter(document);

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            //layoutFormat.Break = PdfLayoutBreakType.FitPage;
            layoutFormat.Layout = PdfLayoutType.Paginate;
            //not fit page then next page
            layoutFormat.Break = PdfLayoutBreakType.FitElement;


            RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            tocresult = new PdfLayoutResult(pageTOC, bounds);

            PdfStringFormat tocformat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Top);
            PdfTextElement indexheader = new PdfTextElement("Table Of Content", largeheaderfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.WordWrap = PdfWordWrapType.Word;

            foreach (var template in designvisit)
            {
                if (template.ProjectDesignTemplatelist.Count > 0)
                {
                    PdfSection SectionContent = document.Sections.Add();
                    PdfPage pageContent = SectionContent.Pages.Add();
                    SectionContent.Template.Top = VisitTemplateHeader(document, details.ProjectDetails.ProjectCode, template.DisplayName, details.ScreeningNumber, details.RandomizationNumber, details.Initial, Convert.ToBoolean(reportSetting.IsScreenNumber), Convert.ToBoolean(reportSetting.IsSubjectNumber), Convert.ToBoolean(reportSetting.IsInitial), Convert.ToBoolean(reportSetting.IsSiteCode));
                    DesignTemplateReport(template.ProjectDesignTemplatelist, reportSetting, template.DisplayName, pageContent, details.ProjectDetails.ProjectDesignId);
                }
            }
        }

        private void DesignTemplateReport(IList<ProjectDesignTemplatelist> designtemplate, ReportSettingNew reportSetting, string vistitName, PdfPage sectioncontent, int ProjectDesignId)
        {
            try
            {


                DateTime dDate;
                RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
                PdfLayoutResult result = new PdfLayoutResult(sectioncontent, bounds);

                PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
                //layoutFormat.Break = PdfLayoutBreakType.FitPage;
                layoutFormat.Layout = PdfLayoutType.Paginate;
                layoutFormat.Break = PdfLayoutBreakType.FitElement;

                var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);

                PdfBookmark bookmark = AddBookmark(result, $"{vistitName}", true);
                foreach (var designt in designtemplate.OrderBy(i => i.DesignOrder))
                {

                    string repeatSeqno = String.IsNullOrEmpty(designt.RepeatSeqNo.ToString()) ? " " : "." + designt.RepeatSeqNo.ToString();
                    AddSection(bookmark, result, $"{designt.DesignOrder.ToString()} {repeatSeqno} {designt.TemplateName}");
                    result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    if (!reportSetting.IsSectionDisplay)
                    {
                        if (reportSetting.AnnotationType == true)
                        {
                            result = AddString($"{designt.DesignOrder.ToString()} {repeatSeqno} {designt.TemplateName} -{designt.Domain.DomainName}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);

                        }
                        else
                        {
                            result = AddString($"{designt.DesignOrder.ToString()} {repeatSeqno} {designt.TemplateName}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);

                        }
                    }
                    else
                    {
                        if (reportSetting.AnnotationType == true)
                        {
                            result = AddString($"{repeatSeqno} {designt.TemplateName} -{designt.Domain.DomainName}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);

                        }
                        else
                        {
                            result = AddString($"{repeatSeqno} {designt.TemplateName}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);
                        }
                    }

                    string notes = "";
                    for (int n = 0; n < designt.TemplateNotes.Count; n++)
                    {
                        if (designt.TemplateNotes[n].IsPreview)
                            notes += designt.TemplateNotes[n].Notes + "\n ";
                    }
                    if (!string.IsNullOrEmpty(notes))
                        result = AddString($"Note(s):\n{notes}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, 400, result.Page.GetClientSize().Height), PdfBrushes.Black, italicfont, layoutFormat);
                    result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);
                    result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 10, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);
                    if (!reportSetting.IsSectionDisplay)
                        AddString("Sr# ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);
                    AddString("Question", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Bottom + 20, 100, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);
                    result = AddString("Answers", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Bottom + 20, 100, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);

                    PdfPen pen = new PdfPen(Color.Gray, 1f);
                    result.Page.Graphics.DrawLine(pen, 0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Bounds.Y + 20);

                    result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 10, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                    //var variabledetails = _context.ProjectDesignVariable.Where(x => x.ProjectDesignTemplateId == designt.Id && x.DeletedDate == null).Include(x => x.Values).Include(x => x.Remarks).Include(x => x.Unit).ToList();
                    foreach (var item in designt.ProjectDesignVariable.OrderBy(i => i.DesignOrder).GroupBy(x => x.VariableCategoryName).Select(y => y.Key))
                    {
                        var category = item;
                        var variableList = designt.ProjectDesignVariable.Where(x => x.VariableCategoryName == item).OrderBy(i => i.DesignOrder).ToList();

                        if (!string.IsNullOrEmpty(category))
                        {
                            // result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                            result = AddStringCategory($"{category}", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Y, 400, 15), PdfBrushes.Black, categoryfont, layoutFormat);

                        }
                        foreach (var variable in variableList)
                        {
                            result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                            string Variablenotes = String.IsNullOrEmpty(variable.Note) ? "" : variable.Note;
                            if (!string.IsNullOrEmpty(Variablenotes))
                                Variablenotes = "Note(s): " + Variablenotes;

                            string annotation = String.IsNullOrEmpty(variable.Annotation) ? " " : $"[{variable.Annotation}]";
                            string CollectionAnnotation = String.IsNullOrEmpty(variable.CollectionAnnotation) ? " " : $"({variable.CollectionAnnotation})";
                            if (reportSetting.AnnotationType == true)
                                result = AddString($"{variable.VariableName}\n {annotation}   {CollectionAnnotation} \n {Variablenotes} \n ", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            else
                                result = AddString($"{variable.VariableName} \n {Variablenotes} \n", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            PdfLayoutResult secondresult = result;
                            if (!reportSetting.IsSectionDisplay)
                                AddString($"{designt.DesignOrder.ToString()}.{variable.DesignOrder.ToString()}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                            if (variable.Unit != null)
                            {
                                if (reportSetting.AnnotationType == true)
                                    AddString($"{variable.Unit.UnitName} \n {variable.Unit.UnitAnnotation}", result.Page, new Syncfusion.Drawing.RectangleF(410, result.Bounds.Y - 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                                else
                                    AddString(variable.Unit.UnitName, result.Page, new Syncfusion.Drawing.RectangleF(410, result.Bounds.Y - 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);

                            }
                            if (variable.IsNa)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, "singlecheckbox");
                                    checkField.Bounds = new RectangleF(410, result.Bounds.Y + 10, 10, 10);
                                    checkField.Style = PdfCheckBoxStyle.Check;
                                    document.Form.Fields.Add(checkField);
                                    AddString("Na", result.Page, new Syncfusion.Drawing.RectangleF(425, result.Bounds.Y + 10, 50, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                                }
                                else
                                {
                                    PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, "singlecheckbox");
                                    checkField.Bounds = new RectangleF(410, result.Bounds.Y + 10, 10, 10);
                                    checkField.Style = PdfCheckBoxStyle.Check;
                                    if (variable.ScreeningIsNa)
                                        checkField.Checked = true;
                                    checkField.ReadOnly = true;
                                    document.Form.Fields.Add(checkField);
                                    AddString("Na", result.Page, new Syncfusion.Drawing.RectangleF(425, result.Bounds.Y + 10, 50, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);

                                }
                            }
                            if (variable.CollectionSource == CollectionSources.TextBox || variable.CollectionSource == CollectionSources.MultilineTextBox)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, variable.Id.ToString());
                                    textBoxField.Bounds = new RectangleF(300, result.Bounds.Y, 100, 20);
                                    textBoxField.BorderWidth = 1;
                                    textBoxField.BorderColor = new PdfColor(Color.Gray);
                                    textBoxField.Multiline = true;
                                    document.Form.Fields.Add(textBoxField);
                                }
                                else
                                {
                                    PdfLayoutFormat multitextlayoutFormat = new PdfLayoutFormat();
                                    multitextlayoutFormat.Layout = PdfLayoutType.Paginate;
                                    multitextlayoutFormat.Break = PdfLayoutBreakType.FitColumnsToPage;
                                    result = AddString(variable.ScreeningValue == null ? " " : variable.ScreeningValue, result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y, 150, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, multitextlayoutFormat);
                                }
                                // result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Bottom, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            else if (variable.CollectionSource == CollectionSources.ComboBox)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    PdfComboBoxField comboBox = new PdfComboBoxField(result.Page, variable.Id.ToString());
                                    comboBox.Bounds = new RectangleF(300, result.Bounds.Y, 100, 20);
                                    comboBox.BorderColor = new PdfColor(Color.Gray);
                                    string ValueName = "";
                                    foreach (var value in variable.Values)
                                    {
                                        ValueName = value.ValueName;
                                        comboBox.Items.Add(new PdfListFieldItem(value.ValueName, value.Id.ToString()));
                                    }
                                    document.Form.Fields.Add(comboBox);
                                    document.Form.SetDefaultAppearance(false);
                                }
                                else
                                {
                                    int Id;
                                    bool isNumeric = int.TryParse(variable.ScreeningValue, out Id);
                                    string dropdownvalue = "";
                                    if (isNumeric)
                                        dropdownvalue = variable.Values.Where(x => x.Id == Id).Select(x => x.ValueName).FirstOrDefault();
                                    SizeF size = regularfont.MeasureString($"{dropdownvalue}");
                                    result = AddString($"{dropdownvalue}", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y, 180, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                }

                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y + 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            else if (variable.CollectionSource == CollectionSources.RadioButton || variable.CollectionSource == CollectionSources.NumericScale)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    foreach (var value in variable.Values.OrderBy(i => i.SeqNo))
                                    {
                                        result = AddString($"{ value.ValueName} {value.Label}", result.Page, new Syncfusion.Drawing.RectangleF(320, result.Bounds.Y, 180, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                        PdfRadioButtonListField radioList = new PdfRadioButtonListField(result.Page, variable.Id.ToString());
                                        PdfRadioButtonListItem radioItem1 = new PdfRadioButtonListItem(value.ValueCode.ToString());
                                        radioItem1.Bounds = new RectangleF(300, result.Bounds.Y, 13, 13);
                                        radioList.Items.Add(radioItem1);
                                        document.Form.Fields.Add(radioList);
                                        result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Bottom + 10, 180, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                    }
                                }
                                else
                                {
                                    int Id;
                                    bool isNumeric = int.TryParse(variable.ScreeningValue, out Id);
                                    string variblevaluename = "";
                                    if (isNumeric)
                                        variblevaluename = variable.Values.Where(x => x.Id == Id).Select(x => x.ValueName).SingleOrDefault();
                                    foreach (var value in variable.Values.OrderBy(x => x.SeqNo))
                                    {
                                        result = AddString($"{ value.ValueName} { value.Label }", result.Page, new Syncfusion.Drawing.RectangleF(320, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                        PdfRadioButtonListField radioList = new PdfRadioButtonListField(result.Page, variable.Id.ToString());
                                        document.Form.Fields.Add(radioList);

                                        PdfRadioButtonListItem radioItem1 = new PdfRadioButtonListItem(value.ValueCode.ToString());
                                        radioItem1.Bounds = new RectangleF(300, result.Bounds.Y, 13, 13);
                                        radioList.Items.Add(radioItem1);
                                        radioList.ReadOnly = true;
                                        if (value.ValueName == variblevaluename)
                                            radioList.SelectedIndex = 0;
                                        result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(320, result.Bounds.Bottom + 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                    }
                                }
                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y, 180, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            else if (variable.CollectionSource == CollectionSources.MultiCheckBox)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    foreach (var value in variable.Values.OrderBy(i => i.SeqNo))
                                    {
                                        result = AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(320, result.Bounds.Y, 180, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                        PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, "UG");
                                        checkField.Bounds = new RectangleF(300, result.Bounds.Y, 15, 15);
                                        checkField.Style = PdfCheckBoxStyle.Check;
                                        //checkField.Checked = true;
                                        document.Form.Fields.Add(checkField);
                                        result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Bottom + 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                    }
                                }
                                else
                                {
                                    var variablename = variable.ValueChild.Where(x => x.Value == "true").Select(x => x.ValueName).ToList();

                                    foreach (var value in variable.Values.OrderBy(i => i.SeqNo))
                                    {
                                        result = AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(320, result.Bounds.Y, 180, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                        PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, value.ValueCode.ToString());
                                        checkField.Bounds = new RectangleF(300, result.Bounds.Y, 15, 15);
                                        checkField.Style = PdfCheckBoxStyle.Check;
                                        checkField.ReadOnly = true;
                                        if (variablename.ToList().Contains(value.ValueName))
                                            checkField.Checked = true;
                                        document.Form.Fields.Add(checkField);

                                        result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Bottom + 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                    }
                                }
                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            else if (variable.CollectionSource == CollectionSources.CheckBox)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    foreach (var value in variable.Values)
                                    {
                                        result = AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(320, result.Bounds.Y, 180, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                        PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, "singlecheckbox");
                                        checkField.Bounds = new RectangleF(300, result.Bounds.Y, 15, 15);
                                        checkField.Style = PdfCheckBoxStyle.Check;
                                        //checkField.ReadOnly = true;
                                        document.Form.Fields.Add(checkField);
                                    }
                                }
                                else
                                {
                                    foreach (var value in variable.Values)
                                    {
                                        result = AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(320, result.Bounds.Y, 180, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                        PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, "singlecheckbox");
                                        checkField.Bounds = new RectangleF(300, result.Bounds.Y, 15, 15);
                                        checkField.Style = PdfCheckBoxStyle.Check;
                                        if (!String.IsNullOrEmpty(variable.ScreeningValue))
                                        {
                                            if (variable.ScreeningValue == "true")
                                            {
                                                checkField.Checked = true;
                                            }
                                        }
                                        checkField.ReadOnly = true;
                                        document.Form.Fields.Add(checkField);
                                    }
                                }
                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y + 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            else if (variable.CollectionSource == CollectionSources.Date)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    PdfTextBoxField field = new PdfTextBoxField(result.Page, "datePick");
                                    field.Bounds = new RectangleF(300, result.Bounds.Y, 100, 20);
                                    //field.Actions.KeyPressed = new PdfJavaScriptAction("AFDate_KeystrokeEx(\"m/d/yy\")");
                                    //field.Actions.Format = new PdfJavaScriptAction("AFDate_FormatEx(\"m/d/yy\")");
                                    //field.Text = textvalue;
                                    document.Form.Fields.Add(field);
                                }
                                else
                                {
                                    var dt = !string.IsNullOrEmpty(variable.ScreeningValue) ? DateTime.TryParse(variable.ScreeningValue, out dDate) ? DateTime.Parse(variable.ScreeningValue).UtcDateTime().ToString(GeneralSettings.DateFormat) : variable.ScreeningValue : "";

                                    PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, variable.Id.ToString());
                                    textBoxField.Bounds = new RectangleF(300, result.Bounds.Y, 100, 20);
                                    textBoxField.Text = dt;
                                    textBoxField.ReadOnly = true;
                                    document.Form.Fields.Add(textBoxField);
                                }
                                AddString(GeneralSettings.DateFormat.ToUpper(), result.Page, new Syncfusion.Drawing.RectangleF(410, result.Bounds.Y - 10, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y - 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            else if (variable.CollectionSource == CollectionSources.DateTime)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, variable.Id.ToString());
                                    textBoxField.Bounds = new RectangleF(300, result.Bounds.Y, 100, 20);
                                    document.Form.Fields.Add(textBoxField);
                                }
                                else
                                {
                                    var dttime = !string.IsNullOrEmpty(variable.ScreeningValue) ? DateTime.TryParse(variable.ScreeningValue, out dDate) ? DateTime.Parse(variable.ScreeningValue).UtcDateTime().ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variable.ScreeningValue : "";

                                    PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, variable.Id.ToString());
                                    textBoxField.Bounds = new RectangleF(300, result.Bounds.Y, 100, 20);
                                    textBoxField.Text = dttime;
                                    textBoxField.ReadOnly = true;
                                    document.Form.Fields.Add(textBoxField);

                                }
                                AddString(GeneralSettings.DateFormat.ToUpper() + " " + GeneralSettings.TimeFormat.ToUpper(), result.Page, new Syncfusion.Drawing.RectangleF(410, result.Bounds.Y - 10, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y - 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            else if (variable.CollectionSource == CollectionSources.PartialDate)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "PartialDate");
                                    textBoxField.Bounds = new RectangleF(300, result.Bounds.Y, 100, 20);
                                    document.Form.Fields.Add(textBoxField);
                                }
                                else
                                {
                                    PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "PartialDate");
                                    textBoxField.Bounds = new RectangleF(300, result.Bounds.Y, 100, 20);
                                    textBoxField.Text = variable.ScreeningValue == null ? "" : variable.ScreeningValue;
                                    textBoxField.ReadOnly = true;
                                    document.Form.Fields.Add(textBoxField);
                                }
                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y + 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            else if (variable.CollectionSource == CollectionSources.Time)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "Time");
                                    textBoxField.Bounds = new RectangleF(300, result.Bounds.Y, 100, 20);
                                    document.Form.Fields.Add(textBoxField);
                                    result = AddString(GeneralSettings.TimeFormat.ToUpper(), result.Page, new Syncfusion.Drawing.RectangleF(410, result.Bounds.Y + 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                                }
                                else
                                {
                                    //var time = !string.IsNullOrEmpty(variable.ScreeningValue) ? DateTime.Parse(variable.ScreeningValue).UtcDateTime().ToString(GeneralSettings.TimeFormat, CultureInfo.InvariantCulture) : "";

                                    PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "Time");
                                    textBoxField.Bounds = new RectangleF(300, result.Bounds.Y, 100, 20);
                                    textBoxField.Text = variable.ScreeningValue;
                                    textBoxField.ReadOnly = true;
                                    document.Form.Fields.Add(textBoxField);
                                    AddString(GeneralSettings.TimeFormat.ToUpper(), result.Page, new Syncfusion.Drawing.RectangleF(410, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                                }
                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y + 10, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            else if (variable.CollectionSource == CollectionSources.HorizontalScale)
                            {
                                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                                {
                                    List<string> _points = new List<string>();
                                    int lowrangevalue = String.IsNullOrEmpty(variable.LowRangeValue) ? 0 : Convert.ToInt32(variable.LowRangeValue);
                                    int highragnevalue = Convert.ToInt32(variable.HighRangeValue);
                                    int stepvalue = String.IsNullOrEmpty(variable.DefaultValue) ? 1 : Convert.ToInt32(variable.DefaultValue);
                                    //logic
                                    for (int i = lowrangevalue; i <= highragnevalue; i++)
                                    {
                                        if ((i % stepvalue) == 0)
                                            _points.Add(i.ToString());
                                    }
                                    float xPos = 300;
                                    result.Page.Graphics.DrawLine(PdfPens.Black, new PointF(xPos, result.Bounds.Y + 20), new PointF(xPos + 180, result.Bounds.Y + 20));
                                    float yPos = result.Bounds.Y + 10;
                                    float increment = (float)180 / (_points.Count - 1);
                                    float smallyPos = result.Bounds.Y + 5;
                                    for (int i = 0; i < _points.Count; i++)
                                    {
                                        //if ((i % 5) == 0)
                                        //{
                                        result.Page.Graphics.DrawLine(PdfPens.Black, new PointF(xPos, yPos), new PointF(xPos, yPos + 20));
                                        result.Page.Graphics.DrawString(_points[i], new PdfStandardFont(PdfFontFamily.TimesRoman, 8), PdfBrushes.Black, new PointF(xPos - 2, yPos + 25));
                                        //}
                                        //else
                                        //{
                                        //    result.Page.Graphics.DrawLine(PdfPens.Black, new PointF(xPos, yPos), new PointF(xPos, smallyPos + 20));
                                        //}
                                        xPos = xPos + increment;
                                    }
                                }
                                else
                                {
                                    result = AddString(variable.ScreeningValue, result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                }
                                result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(300, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            else
                            {
                                result = AddString(variable.CollectionSource.ToString(), result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            }
                            //result = AddString("--last line ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            PdfLayoutResult thirdresult = result;
                            if (secondresult.Page == thirdresult.Page)
                                if (secondresult.Bounds.Bottom > thirdresult.Bounds.Bottom)
                                    if (thirdresult.Bounds.Height < secondresult.Bounds.Height)
                                        result = AddString(" ", secondresult.Page, new Syncfusion.Drawing.RectangleF(0, secondresult.Bounds.Bottom, secondresult.Page.GetClientSize().Width, secondresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                            //data
                            result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        }

                    }
                    result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);


                    PdfPen pen1 = new PdfPen(Color.Gray, 1f);
                    result.Page.Graphics.DrawLine(pen1, 0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Bounds.Y + 20);
                    result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);

                    string notesb = "";
                    for (int n = 0; n < designt.TemplateNotesBottom.Count; n++)
                    {
                        if (designt.TemplateNotesBottom[n].IsPreview)
                            notesb += designt.TemplateNotesBottom[n].Notes + "\n ";
                    }
                    if (!string.IsNullOrEmpty(notesb))
                        result = AddString($"Notes:\n{notesb}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, 400, result.Page.GetClientSize().Height), PdfBrushes.Black, italicfont, layoutFormat);



                    var workflowlevel = _context.ProjectWorkflow.Where(x => x.ProjectDesignId == ProjectDesignId).Include(x => x.Levels).ToList();
                    foreach (var workflow in workflowlevel)
                    {
                        var levels = workflow.Levels;
                        foreach (var level in levels)
                        {
                            if (level.IsElectricSignature)
                            {
                                var signature = designt.ScreeningTemplateReview.Where(s => s.ReviewLevel > level.LevelNo - 1 && s.RoleId == level.SecurityRoleId).LastOrDefault();
                                if (signature != null)
                                {
                                    result = AddString($"{ signature.CreatedByUser}  ({signature.RoleName})", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, 400, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);
                                    result = AddString(Convert.ToDateTime(signature.CreatedDate).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat), result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, 400, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                    result = AddString("I, hereby understand, that applying my electronic signature in the electronic system is equivalent to utilising my hand written signature", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, 400, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                    result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                    if (result.Bounds.Bottom + 30 >= 770)
                                        result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 30, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                                }
                            }
                        }
                    }

                    //if (designt.ScreeningTemplateReview != null)
                    //{
                    //    foreach (var signature in designt.ScreeningTemplateReview)
                    //    {
                    //        result = AddString($"{ signature.CreatedByUser}  ({signature.RoleName})", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, 400, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);
                    //        result = AddString(Convert.ToDateTime(signature.CreatedDate).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat), result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, 400, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    //        result = AddString("I, hereby understand, that applying my electronic signature in the electronic system is equivalent to utilising my hand written signature", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, 400, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    //        result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    //        if (result.Bounds.Bottom + 30 >= 770)
                    //            result = AddString(" ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 30, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                    //    }
                    //}

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private void SaveFile(ReportSettingNew reportSetting, FileSaveInfo fileInfo, MemoryStream memoryStream, DossierReportDto item, string ParentProjectCode)
        {
            string filePath = "";
            if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
            {
                //fileInfo.ParentFolderName = projectDetails.FirstOrDefault().ProjectDetails.ProjectCode + "_" + DateTime.Now.Ticks;
                fileInfo.ParentFolderName = ParentProjectCode + "_" + DateTime.Now.Ticks;
                fileInfo.FileName = fileInfo.ParentFolderName.Replace("/", "") + ".pdf";
                filePath = System.IO.Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName, fileInfo.FileName);
            }
            else
            {
                fileInfo.ParentFolderName = fileInfo.ParentFolderName.Trim().Replace(" ", "").Replace("/", "");
                fileInfo.FileName = item.Initial.Replace("/", "") + ".pdf";
                string fileName = fileInfo.FileName + ".pdf";
                fileInfo.ChildFolderName = item.ProjectDetails.ProjectCode;
                filePath = System.IO.Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName, fileInfo.ChildFolderName, fileName);
            }
            bool exists = Directory.Exists(filePath);
            if (!exists)
                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                    Directory.CreateDirectory(Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName));
                else
                    Directory.CreateDirectory(Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName, fileInfo.ChildFolderName));

            using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                memoryStream.WriteTo(fs);
            }
        }

        private void UpdateJobStatus(JobMonitoring jobMonitoring, FileSaveInfo fileInfo, string ParentProjectCode, string ParentProjectName, string documentUrl)
        {
            jobMonitoring.CompletedTime = _jwtTokenAccesser.GetClientDate();
            jobMonitoring.JobStatus = JobStatusType.Completed;
            jobMonitoring.FolderPath = System.IO.Path.Combine(documentUrl, fileInfo.ModuleName, fileInfo.FolderType);
            jobMonitoring.FolderName = fileInfo.ParentFolderName + ".zip";
            var completeJobMonitoring = _reportBaseRepository.CompleteJobMonitoring(jobMonitoring);

            string Zipfilename = Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName);
            ZipFile.CreateFromDirectory(Zipfilename, Zipfilename + ".zip");
            Directory.Delete(Zipfilename, true);

            var user = _userRepository.Find(_jwtTokenAccesser.UserId);
            //var ProjectName = projectDetails.FirstOrDefault().ProjectDetails.ProjectCode + "-" + projectDetails.FirstOrDefault().ProjectDetails.ProjectName;
            var ProjectName = ParentProjectCode + "-" + ParentProjectName;
            string asa = Path.Combine(documentUrl, fileInfo.ModuleName, fileInfo.FolderType, jobMonitoring.FolderName);
            var linkOfPdf = "<a href='" + asa + "'>Click Here</a>";
            _emailSenderRespository.SendPdfGeneratedEMail(user.Email, _jwtTokenAccesser.UserName, ProjectName, linkOfPdf);
        }

        private void SyncFile(string DocumentName, ReportSettingNew reportSetting, MemoryStream memoryStream)
        {
            SyncConfigurationParameterDto parameterDto = new SyncConfigurationParameterDto();
            parameterDto.CountryId = Convert.ToInt32(reportSetting.CountryId);
            parameterDto.ProjectId = reportSetting.ProjectId;
            parameterDto.SiteId = reportSetting.SitesId;
            parameterDto.ReportCode = reportSetting.ReportCode;
            string DocumentPath = _syncConfigurationMasterRepository.SaveArtifactDocument(DocumentName, parameterDto);
            string fullPath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), DocumentPath, DocumentName);
            Directory.CreateDirectory(Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), DocumentPath));
            using (System.IO.FileStream fs = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
            {
                memoryStream.WriteTo(fs);
            }
        }
    }

}
