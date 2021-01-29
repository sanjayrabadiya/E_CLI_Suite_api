using BoldReports.Web;
using BoldReports.Writer;
using GSC.Data.Dto.Common;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Report;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Report;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Report.Common;
using GSC.Respository.Client;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
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
        private readonly IProjectDesignRepository _projectDesignRepository;


        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;// GetVisitsByProjectDesignId
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IReportBaseRepository _reportBaseRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IGSCContext _context;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        //private PdfFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
        private readonly PdfFont watermarkerfornt = new PdfStandardFont(PdfFontFamily.TimesRoman, 120, PdfFontStyle.Bold);
        private readonly PdfFont largeheaderfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);
        private readonly PdfFont headerfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 14, PdfFontStyle.Bold);
        private readonly PdfFont regularfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
        private readonly PdfFont smallfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 8);

        // private float yPosition { get; set; }
        //string str = "Threads are relatively lightweight processes responsible for multitasking within a single "
        //         + "application. The System.Threading namespace provides a wealth of classes and interfaces to "
        //         + "manage multithreaded programming. The majority of programmers might never need to manage "
        //         + "threads explicitly, however, because the Common Language Runtime (CLR) abstracts much of the "
        //         + "threading support into classes that greatly simplify most threading tasks. For example, "
        //         + " you will see how to create multithreaded reading and writing streams without resorting to "
        //         + "managing the threads yourself.";

        private PdfDocument document = null;
        private PdfLayoutResult tocresult = null;

        public ReportSyncfusion(IHostingEnvironment hostingEnvironment, IProjectDesignRepository projectDesignRepository, IProjectDesignVisitRepository projectDesignVisitRepository,
        IProjectDesignTemplateRepository projectDesignTemplateRepository, IProjectDesignVariableRepository projectDesignVariableRepository, IUploadSettingRepository uploadSettingRepository, IReportBaseRepository reportBaseRepository, ICompanyRepository companyRepository,
        IClientRepository clientRepository, IGSCContext context, IAppSettingRepository appSettingRepository, IJwtTokenAccesser jwtTokenAccesser)
        {
            _hostingEnvironment = hostingEnvironment;
            _projectDesignRepository = projectDesignRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _reportBaseRepository = reportBaseRepository;
            _companyRepository = companyRepository;
            _clientRepository = clientRepository;
            _context = context;
            _appSettingRepository = appSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public void BlankReportGenerate(ReportSettingNew reportSetting, JobMonitoring jobMonitoring)
        {
            var projectdetails = _projectDesignRepository.FindByInclude(i => i.ProjectId == reportSetting.ProjectId, i => i.Project).SingleOrDefault();
            var projectDesignvisit = _projectDesignVisitRepository.GetVisitsByProjectDesignId(projectdetails.Id);


            document = new PdfDocument();
            document.PageSettings.Margins.Top = Convert.ToInt32(reportSetting.TopMargin * 100);
            document.PageSettings.Margins.Bottom = Convert.ToInt32(reportSetting.BottomMargin * 100);
            document.PageSettings.Margins.Left = Convert.ToInt32(reportSetting.LeftMargin * 100);
            document.PageSettings.Margins.Right = Convert.ToInt32(reportSetting.RightMargin * 100);            

            DesignVisit(projectDesignvisit, reportSetting, projectdetails.Project.ProjectCode);


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
                    graphics.DrawString("Draft", watermarkerfornt, PdfPens.LightBlue, PdfBrushes.LightBlue, new PointF(-150, 500));
                    graphics.Restore();
                }
            }
            MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream);

            var base_URL = _uploadSettingRepository.All.OrderByDescending(x => x.Id).FirstOrDefault().DocumentPath;
            //reportSettingNew.TimezoneoffSet = reportSettingNew.TimezoneoffSet * (-1);
            FileSaveInfo fileInfo = new FileSaveInfo();
            fileInfo.Base_URL = base_URL;
            fileInfo.ModuleName = Enum.GetName(typeof(JobNameType), jobMonitoring.JobName);
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();

            fileInfo.FolderType = Enum.GetName(typeof(DossierPdfStatus), jobMonitoring.JobDetails);
            //fileInfo.ParentFolderName = projectdetails.Project.ProjectCode + "-" + projectdetails.Project.ProjectName + "_" + DateTime.Now.Ticks;
            fileInfo.ParentFolderName = projectdetails.Project.ProjectCode + "_" + DateTime.Now.Ticks;
            fileInfo.FileName = fileInfo.ParentFolderName.Replace("/", "") + ".pdf";
            fileInfo.ParentFolderName = fileInfo.ParentFolderName.Trim().Replace(" ", "").Replace("/", "");


            string filePath = System.IO.Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName, fileInfo.FileName);

            bool exists = Directory.Exists(filePath);
            if (!exists)
                Directory.CreateDirectory(Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName));

            using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                memoryStream.WriteTo(fs);
            }

            //// add job Monitor
            jobMonitoring.CompletedTime = DateTime.Now.UtcDateTime();
            jobMonitoring.JobStatus = JobStatusType.Completed;
            jobMonitoring.FolderPath = System.IO.Path.Combine(documentUrl, fileInfo.ModuleName, fileInfo.FolderType);
            jobMonitoring.FolderName = fileInfo.ParentFolderName + ".zip";
            var completeJobMonitoring = _reportBaseRepository.CompleteJobMonitoring(jobMonitoring);


            string Zipfilename = Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName);
            ZipFile.CreateFromDirectory(Zipfilename, Zipfilename + ".zip");
            Directory.Delete(Zipfilename, true);
        }

        private PdfPageTemplateElement AddHeader(PdfDocument doc, string studyName, bool isClientLogo, bool isCompanyLogo)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 100);

            //Create a page template
            PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 24);
            float doubleHeight = font.Height * 2;
            Color activeColor = Color.FromArgb(44, 71, 120);
            SizeF imageSize = new SizeF(50f, 50f);

            //string basePath = _hostingEnvironment.WebRootPath;



            var imagePath = _uploadSettingRepository.GetImagePath();
            if (isCompanyLogo)
            {
                var companylogopath = _companyRepository.All.Select(x => x.Logo).FirstOrDefault();
                if (File.Exists($"{imagePath}/{companylogopath}") && !String.IsNullOrEmpty(companylogopath))
                {
                    FileStream logoinputstream = new FileStream($"{imagePath}/{companylogopath}", FileMode.Open, FileAccess.Read);
                    PdfImage img = new PdfBitmap(logoinputstream);
                    var companylogo = new PointF(20, 40);
                    header.Graphics.DrawImage(img, companylogo, imageSize);
                }
            }
            if (isClientLogo)
            {
                var clientlogopath = _clientRepository.All.Select(x => x.Logo).FirstOrDefault();
                if (File.Exists($"{imagePath}/{clientlogopath}") && !String.IsNullOrEmpty(clientlogopath))
                {
                    FileStream logoinputstream = new FileStream($"{imagePath}/{clientlogopath}", FileMode.Open, FileAccess.Read);
                    PdfImage img = new PdfBitmap(logoinputstream);
                    var imageLocation = new PointF(doc.Pages[0].GetClientSize().Width - imageSize.Width - 20, 40);
                    header.Graphics.DrawImage(img, imageLocation, imageSize);
                }
            }

            PdfSolidBrush brush = new PdfSolidBrush(activeColor);
            PdfPen pen = new PdfPen(Color.DarkBlue, 3f);
            font = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);

            //Set formattings for the text
            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Center;
            format.LineAlignment = PdfVerticalAlignment.Middle;

            //Draw title
            header.Graphics.DrawString("Clinvigilant", font, brush, new RectangleF(0, 0, header.Width, header.Height), format);
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
            // header.Graphics.DrawLine(pen, 0, header.Height - 3, header.Width, header.Height - 3);
            header.Graphics.DrawLine(pen, 0, header.Height, header.Width, header.Height);

            return header;
        }

        private PdfPageTemplateElement AddFooter(PdfSection section)
        {
            RectangleF rect = new RectangleF(0, 0, section.Pages[0].GetClientSize().Width, 50);

            //Create a page template
            PdfPageTemplateElement footer = new PdfPageTemplateElement(rect);
            PdfFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 8, PdfFontStyle.Bold);

            PdfSolidBrush brush = new PdfSolidBrush(Color.Black);

            //Create the page number field
            PdfPageNumberField pageNumber = new PdfPageNumberField(font, brush);

            //Create the page count field
            PdfPageCountField count = new PdfPageCountField(font, brush);

            //Add the fields in the composite fields
            PdfCompositeField compositeField = new PdfCompositeField(font, brush, "Page {0} of {1}", pageNumber, count);
            compositeField.Bounds = footer.Bounds;

            //Draw the composite field in the footer
            compositeField.Draw(footer.Graphics, new PointF(470, 30));

            //Drawing Line
            PdfPen pen = new PdfPen(Color.Black, 1.0f);
            footer.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);

            return footer;
        }

        private PdfLayoutResult AddString(string note, PdfPage page, RectangleF position, PdfBrush brush, PdfFont font, PdfLayoutFormat pdfLayoutFormat)
        {
            PdfTextElement richTextElement = new PdfTextElement(note, font, brush);
            //Draws String       
            PdfStringFormat stringFormat = new PdfStringFormat();
            stringFormat.MeasureTrailingSpaces = true;
            stringFormat.WordWrap = PdfWordWrapType.Word;
            richTextElement.StringFormat = stringFormat;


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
            AddTableOfcontents(sectionpage, title, isVisit);
            // Adding bookmark with named destination
            PdfNamedDestination namedDestination = new PdfNamedDestination(title);
            namedDestination.Destination = new PdfDestination(sectionpage.Page, new PointF(0, sectionpage.Bounds.Y));
            namedDestination.Destination.Mode = PdfDestinationMode.FitToPage;
            document.NamedDestinationCollection.Add(namedDestination);
            bookmarks.NamedDestination = namedDestination;
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
            pageNumber.Draw(tocresult.Page, new PointF(490, tocresult.Bounds.Y));

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

        private void DesignVisit(IList<DropDownDto> designvisit, ReportSettingNew reportSetting, string projectCode)
        {
            PdfSection SectionTOC = document.Sections.Add();

            PdfPage pageTOC = SectionTOC.Pages.Add();

            document.Template.Top = AddHeader(document, projectCode, Convert.ToBoolean(reportSetting.IsClientLogo), Convert.ToBoolean(reportSetting.IsCompanyLogo));
            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            //layoutFormat.Break = PdfLayoutBreakType.FitPage;
            layoutFormat.Layout = PdfLayoutType.Paginate;
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


            //document.Template.Bottom = AddFooter(document);
            var scrrening = _context.ScreeningEntry.Where(x => x.ProjectId == reportSetting.ProjectId)
              .Include(x => x.ScreeningVisit).ThenInclude(x => x.ScreeningTemplates).ThenInclude(x => x.ScreeningTemplateValues).ToList();

            foreach (var template in designvisit)
            {
                PdfSection SectionContent = document.Sections.Add();
                PdfPage pageContent = SectionContent.Pages.Add();
                SectionContent.Template.Top = VisitTemplateHeader(document, template.Value, "", "", "");
                SectionContent.Template.Bottom = AddFooter(SectionContent);
                var projecttemplate = _projectDesignTemplateRepository.FindByInclude(x => x.ProjectDesignVisitId == template.Id, x => x.ProjectDesignTemplateNote).ToList();
                DesignTemplate(projecttemplate, reportSetting, template.Value, pageContent);


            }
        }
        private void DesignVisitData(List<ScreeningVisit> screeningVisits, ReportSettingNew reportSetting, string projectCode, ScreeningEntry screeningEntry)
        {
            PdfSection SectionTOC = document.Sections.Add();
            //SectionTOC.PageSettings.Margins.Top = 0;
            //SectionTOC.PageSettings.Margins.Bottom = 0;
            //SectionTOC.PageSettings.Margins.Left = 0;
            //SectionTOC.PageSettings.Margins.Right = 0;

            //PdfPage pageTOC = SectionTOC.Pages.Add();

            //document.Template.Top = AddHeader(document, projectCode, Convert.ToBoolean(reportSetting.IsClientLogo), Convert.ToBoolean(reportSetting.IsCompanyLogo));
            //PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            ////layoutFormat.Break = PdfLayoutBreakType.FitPage;
            //layoutFormat.Layout = PdfLayoutType.Paginate;
            //layoutFormat.Break = PdfLayoutBreakType.FitElement;

            //RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            //tocresult = new PdfLayoutResult(pageTOC, bounds);

            //PdfStringFormat tocformat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Top);
            //PdfTextElement indexheader = new PdfTextElement("Table Of Content", largeheaderfont, PdfBrushes.Black);
            //indexheader.StringFormat = tocformat;
            //tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.WordWrap = PdfWordWrapType.Word;


            ////document.Template.Bottom = AddFooter(document);
            //var scrrening = _context.ScreeningEntry.Where(x => x.ProjectId == reportSetting.ProjectId)
            //  .Include(x => x.ScreeningVisit).ThenInclude(x => x.ScreeningTemplates).ThenInclude(x => x.ScreeningTemplateValues).ToList();



            foreach (var visit in screeningVisits)
            {
                var screeningtemplate = _context.ScreeningTemplate.Where(x => x.Status != ScreeningTemplateStatus.Pending)
                    .Include(x => x.ProjectDesignTemplate).ThenInclude(i => i.ProjectDesignTemplateNote)
                    .Include(x => x.ScreeningTemplateValues).ThenInclude(x => x.ProjectDesignVariable)
                    .ThenInclude(x => x.Unit).Where(x => x.ScreeningVisitId == visit.Id)
                    .OrderBy(x => x.ProjectDesignTemplate.DesignOrder).ToList();

                var visitName = (_jwtTokenAccesser.Language != 1 ?
                visit.ProjectDesignVisit.VisitLanguage.Where(x => x.LanguageId == (int)_jwtTokenAccesser.Language).Select(a => a.Display).FirstOrDefault()
                : visit.ProjectDesignVisit.DisplayName) +
                                         Convert.ToString(visit.RepeatedVisitNumber == null ? "" : "_" + visit.RepeatedVisitNumber);


                PdfSection SectionContent = document.Sections.Add();
                //SectionContent.PageSettings.Margins.Top = 0;
                //SectionContent.PageSettings.Margins.Bottom = 0;
                //SectionContent.PageSettings.Margins.Left = 0;
                //SectionContent.PageSettings.Margins.Right = 0;
                PdfPage pageContent = SectionContent.Pages.Add();
                SectionContent.Template.Top = VisitTemplateHeader(document, visitName, screeningEntry.Randomization.ScreeningNumber, screeningEntry.Randomization.RandomizationNumber, screeningEntry.Randomization.Initial);
                //SectionContent.Template.Bottom = AddFooter(SectionContent);
                //var projecttemplate = _projectDesignTemplateRepository.FindByInclude(x => x.ProjectDesignVisitId == template.Id, x => x.ProjectDesignTemplateNote).ToList();
                DesignTemplateWithData(screeningtemplate.OrderBy(x => x.ProjectDesignTemplate.DesignOrder).ToList(), reportSetting, visitName, pageContent, screeningEntry);

            }
        }

        private void DesignTemplate(IList<ProjectDesignTemplate> designtemplate, ReportSettingNew reportSetting, string vistitName, PdfPage sectioncontent)
        {
            RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            PdfLayoutResult result = new PdfLayoutResult(sectioncontent, bounds);
            int index = 1;

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            //layoutFormat.Break = PdfLayoutBreakType.FitPage;
            layoutFormat.Layout = PdfLayoutType.Paginate;
            layoutFormat.Break = PdfLayoutBreakType.FitElement;

            //document.Form.SetDefaultAppearance(false);

            AddBookmark(result, $"{vistitName}", true);
            PdfBookmark bookmarks = document.Bookmarks.Add(vistitName);
            bookmarks.Destination = new PdfDestination(result.Page, new PointF(0, result.Bounds.Y + 20));
            bookmarks.Destination.Location = new PointF(0, result.Bounds.Y + 20);

            foreach (var designt in designtemplate)
            {
                AddBookmark(result, $"{index}.{designt.TemplateName}", false);
                bookmarks = document.Bookmarks.Add($"{index}.{designt.TemplateName}");
                bookmarks.Destination = new PdfDestination(result.Page, new PointF(0, result.Bounds.Y + 20));
                bookmarks.Destination.Location = new PointF(0, result.Bounds.Y + 20);

                result = AddString($"{index}.{designt.TemplateName} -{designt.TemplateCode}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);
                string notes = "";
                for (int n = 0; n < designt.ProjectDesignTemplateNote.Count; n++)
                {
                    if (designt.ProjectDesignTemplateNote[n].IsPreview)
                        notes += designt.ProjectDesignTemplateNote[n].Note + "\n";
                }
                if (!string.IsNullOrEmpty(notes))
                    result = AddString($"Notes:\n{notes}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, 400, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);

                AddString("Sr# ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);
                AddString("Question", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Bottom + 20, 100, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);
                result = AddString("Answers", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Bottom + 20, 100, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);

                PdfPen pen = new PdfPen(Color.Gray, 1f);
                result.Page.Graphics.DrawLine(pen, 0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Bounds.Y + 20);

                result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                var variabledetails = _projectDesignVariableRepository.GetVariabeAnnotationDropDownForProjectDesign(designt.Id);

                var variablelist = _projectDesignVariableRepository.FindByInclude(t => t.ProjectDesignTemplateId == designt.Id, t => t.Values, t => t.Remarks, t => t.Unit).ToList();
                int level2index = 1;
                foreach (var variable in variabledetails)
                {
                    result = AddString($"{index}.{level2index}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                    var variabled = variablelist.Where(a => a.Id == variable.Id).FirstOrDefault();
                    string annotation = String.IsNullOrEmpty(variabled.Annotation) ? "" : $"[{variabled.Annotation}]";
                    string CollectionAnnotation = String.IsNullOrEmpty(variabled.CollectionAnnotation) ? "" : $"({variabled.CollectionAnnotation})";
                    if (reportSetting.AnnotationType == true)
                        result = AddString($"{variable.Value}\n {annotation}   {CollectionAnnotation}", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    else
                        result = AddString($"{variable.Value}", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                    PdfLayoutResult secondresult = result;




                    if (variabled.Unit != null)
                        AddString(variabled.Unit.UnitName, result.Page, new Syncfusion.Drawing.RectangleF(460, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                    if (variabled.CollectionSource == CollectionSources.TextBox || variabled.CollectionSource == CollectionSources.MultilineTextBox)
                    {
                        //PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, variabled.Id.ToString());
                        //textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        //textBoxField.BorderWidth = 1;
                        //textBoxField.BorderColor = new PdfColor(Color.Gray);
                        //textBoxField.Multiline = true;
                        //// textBoxField.Text = str;
                        //document.Form.Fields.Add(textBoxField);
                        //var exist = ScreeningTemplate.Where(x => x.ScreeningTemplateValues.Any(y => y.ProjectDesignVariableId == variabled.Id)).ToList();
                        //string textvalue = "";
                        //if (exist != null)
                        //{
                        //    var data = ScreeningTemplate.Where(x => x.ScreeningTemplateValues.Where(b => b.ProjectDesignVariableId == variabled.Id).FirstOrDefault().ProjectDesignVariableId == variabled.Id).FirstOrDefault();
                        //    textvalue = data.ScreeningTemplateValues.FirstOrDefault().Value;
                        //}
                        //textvalue = textvalue != null ? textvalue : "";
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.ComboBox)
                    {
                        PdfComboBoxField comboBox = new PdfComboBoxField(result.Page, variabled.Id.ToString());
                        comboBox.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        comboBox.BorderColor = new PdfColor(Color.Gray);
                        string ValueName = "";
                        foreach (var value in variabled.Values)
                        {
                            ValueName = value.ValueName;
                            comboBox.Items.Add(new PdfListFieldItem(value.ValueName, value.Id.ToString()));
                        }
                        document.Form.Fields.Add(comboBox);
                        // comboBox.SelectedIndex = 1;
                        document.Form.SetDefaultAppearance(false);

                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.RadioButton)
                    {
                        PdfRadioButtonListField radioList = new PdfRadioButtonListField(result.Page, $"{index}{level2index}Radio");
                        foreach (var value in variabled.Values)
                        {
                            document.Form.Fields.Add(radioList);
                            PdfRadioButtonListItem radioItem1 = new PdfRadioButtonListItem(value.ValueCode.ToString());
                            radioItem1.Bounds = new RectangleF(350, result.Bounds.Y, 15, 15);
                            radioList.Items.Add(radioItem1);
                            AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(370, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        }
                        //radioList.SelectedIndex = 0;
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.MultiCheckBox)
                    {
                        foreach (var value in variabled.Values)
                        {
                            PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, "UG");
                            checkField.Bounds = new RectangleF(350, result.Bounds.Y, 10, 10);
                            checkField.Style = PdfCheckBoxStyle.Check;
                            checkField.Checked = true;
                            document.Form.Fields.Add(checkField);
                            AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(370, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        }
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.CheckBox)
                    {
                        foreach (var value in variabled.Values)
                        {
                            PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, "singlecheckbox");
                            checkField.Bounds = new RectangleF(350, result.Bounds.Y, 10, 10);
                            checkField.Style = PdfCheckBoxStyle.Check;
                            checkField.Checked = true;
                            document.Form.Fields.Add(checkField);
                            AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(370, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        }
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.Date)
                    {
                        PdfTextBoxField field = new PdfTextBoxField(result.Page, "datePick");
                        field.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        field.Actions.KeyPressed = new PdfJavaScriptAction("AFDate_KeystrokeEx(\"m/d/yy\")");
                        field.Actions.Format = new PdfJavaScriptAction("AFDate_FormatEx(\"m/d/yy\")");
                        //field.Text = textvalue;
                        document.Form.Fields.Add(field);

                        AddString("MM/dd/yyyy", result.Page, new Syncfusion.Drawing.RectangleF(460, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.DateTime)
                    {
                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, variabled.Id.ToString());
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        //textBoxField.Text = textvalue;
                        document.Form.SetDefaultAppearance(true);
                        document.Form.Fields.Add(textBoxField);
                        AddString("", result.Page, new Syncfusion.Drawing.RectangleF(460, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.PartialDate)
                    {
                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "PartialDate");
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        //textBoxField.Text = textvalue;
                        document.Form.Fields.Add(textBoxField);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.Time)
                    {
                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "Time");
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        //textBoxField.Text = textvalue;
                        document.Form.Fields.Add(textBoxField);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else
                    {
                        result = AddString(variabled.CollectionSource.ToString(), result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    PdfLayoutResult thirdresult = result;
                    if (secondresult.Bounds.Bottom > thirdresult.Bounds.Bottom)
                        if (thirdresult.Bounds.Height < secondresult.Bounds.Height)
                            result = AddString("", thirdresult.Page, new Syncfusion.Drawing.RectangleF(0, thirdresult.Bounds.Bottom + 20, thirdresult.Page.GetClientSize().Width, thirdresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        else
                            result = AddString("", secondresult.Page, new Syncfusion.Drawing.RectangleF(0, secondresult.Bounds.Bottom + 20, secondresult.Page.GetClientSize().Width, secondresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                    ++level2index;
                }
                ++index;
            }
            //document.Template.Top = AddHeader(document, vistitName, vistitName);
            //document.Template.Bottom = AddFooter(document);
            //MemoryStream memoryStream = new MemoryStream();
            //document.Save(memoryStream);
            //return memoryStream;

        }




        private void DesignTemplateWithData(IList<ScreeningTemplate> screeningTemplates, ReportSettingNew reportSetting, string vistitName, PdfPage sectioncontent, ScreeningEntry screeningEntry)
        {
            DateTime dDate;
            RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            PdfLayoutResult result = new PdfLayoutResult(sectioncontent, bounds);
            int index = 1;

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            //layoutFormat.Break = PdfLayoutBreakType.FitPage;
            layoutFormat.Layout = PdfLayoutType.Paginate;
            layoutFormat.Break = PdfLayoutBreakType.FitElement;

            //document.Form.SetDefaultAppearance(false);

            AddBookmark(result, $"{vistitName}", true);
            PdfBookmark bookmarks = document.Bookmarks.Add(vistitName);
            bookmarks.Destination = new PdfDestination(result.Page, new PointF(0, result.Bounds.Y + 20));
            bookmarks.Destination.Location = new PointF(0, result.Bounds.Y + 20);

            var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            GeneralSettings.TimeFormat = GeneralSettings.TimeFormat.Replace("a", "tt");

            foreach (var template in screeningTemplates)
            {
                decimal DesignOrder = template.RepeatSeqNo == null ? template.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(template.ProjectDesignTemplate.DesignOrder.ToString() + "." + template.RepeatSeqNo.Value.ToString());

                AddBookmark(result, $"{DesignOrder.ToString()}.{template.ProjectDesignTemplate.TemplateName}", false);
                bookmarks = document.Bookmarks.Add($"{index}.{template.ProjectDesignTemplate.TemplateName}");
                bookmarks.Destination = new PdfDestination(result.Page, new PointF(0, result.Bounds.Y + 20));
                bookmarks.Destination.Location = new PointF(0, result.Bounds.Y + 20);


                result = AddString($"{DesignOrder.ToString()}.{template.ProjectDesignTemplate.TemplateName} -{template.ProjectDesignTemplate.TemplateCode}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);
                string notes = "";
                for (int n = 0; n < template.ProjectDesignTemplate.ProjectDesignTemplateNote.Count; n++)
                {
                    notes += template.ProjectDesignTemplate.ProjectDesignTemplateNote[n].Note + "\n";
                }
                if (!string.IsNullOrEmpty(notes))
                    result = AddString($"Notes:\n{notes}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, 400, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);

                AddString("Sr# ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);
                AddString("Question", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Bottom + 20, 100, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);
                result = AddString("Answers", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Bottom + 20, 100, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);

                PdfPen pen = new PdfPen(Color.Gray, 1f);
                result.Page.Graphics.DrawLine(pen, 0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Bounds.Y + 20);

                result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);


                int level2index = 1;
                foreach (var variable in template.ScreeningTemplateValues)
                {
                    result = AddString($"{DesignOrder.ToString()}.{variable.ProjectDesignVariable.DesignOrder}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                    string annotation = String.IsNullOrEmpty(variable.ProjectDesignVariable.Annotation) ? "" : $"[{variable.ProjectDesignVariable.Annotation}]";
                    string CollectionAnnotation = String.IsNullOrEmpty(variable.ProjectDesignVariable.CollectionAnnotation) ? "" : $"({variable.ProjectDesignVariable.CollectionAnnotation})";

                    string Variablenotes = String.IsNullOrEmpty(variable.ProjectDesignVariable.Note) ? "" : variable.ProjectDesignVariable.Note;
                    if (!string.IsNullOrEmpty(Variablenotes))
                        Variablenotes = "Notes :" + Variablenotes;

                    if (reportSetting.AnnotationType == true)
                        result = AddString($"{variable.ProjectDesignVariable.VariableName}\n {annotation}   {CollectionAnnotation} \n {Variablenotes}", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    else
                        result = AddString($"{variable.Value} \n{Variablenotes}", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                    //result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);

                    //var data = screening.Select(a => a.ScreeningVisit.Select(b => b.ScreeningTemplates.Select(c => c.ScreeningTemplateValues.Where(d => d.ProjectDesignVariableId == variable.Id)))).SingleOrDefault();

                    PdfLayoutResult secondresult = result;

                    if (variable.ProjectDesignVariable.Unit != null)
                        AddString(variable.ProjectDesignVariable.Unit.UnitName, result.Page, new Syncfusion.Drawing.RectangleF(460, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                    if (variable.ProjectDesignVariable.CollectionSource == CollectionSources.TextBox || variable.ProjectDesignVariable.CollectionSource == CollectionSources.MultilineTextBox)
                    {
                        result = AddString(variable.Value == null ? "" : variable.Value, result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        //result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variable.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox)
                    {
                        PdfComboBoxField comboBox = new PdfComboBoxField(result.Page, variable.ProjectDesignVariable.Id.ToString());
                        comboBox.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        comboBox.BorderColor = new PdfColor(Color.Gray);
                        // comboBox.ToolTip = "Job Title";
                        // string ValueName = "";

                        var variablevalue = _context.ProjectDesignVariableValue.Where(b =>
                                                        b.ProjectDesignVariableId == variable.ProjectDesignVariable.Id
                                                        ).ToList();

                        var variblevaluename = _context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                                        b.ProjectDesignVariableId == variable.ProjectDesignVariable.Id &&
                                                        (variable.Value != null || variable.Value != "" ||
                                                        b.Id == Convert.ToInt32(variable.Value))).ValueName;
                        foreach (var value in variablevalue)
                        {
                            //ValueName = value.ValueName;
                            comboBox.Items.Add(new PdfListFieldItem(value.ValueName, value.Id.ToString()));
                        }
                        int cvalue = variablevalue.FindIndex(x => x.ValueName == variblevaluename);
                        //comboBox.Editable = true;
                        //comboBox.ComplexScript = true;
                        document.Form.Fields.Add(comboBox);
                        comboBox.SelectedValue = variblevaluename;
                        document.Form.SetDefaultAppearance(false);

                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variable.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton || variable.ProjectDesignVariable.CollectionSource == CollectionSources.NumericScale)
                    {
                        var variablevalue = _context.ProjectDesignVariableValue.Where(b =>
                                                 b.ProjectDesignVariableId == variable.ProjectDesignVariable.Id
                                                 ).ToList();

                        var variblevaluename = _context.ProjectDesignVariableValue.Where(b =>
                                                        b.ProjectDesignVariableId == variable.ProjectDesignVariable.Id &&
                                                         (variable.Value != null || variable.Value != "" ||
                                                        b.Id == Convert.ToInt32(variable.Value))).ToList();
                        PdfRadioButtonListField radioList = new PdfRadioButtonListField(result.Page, $"{index}{level2index}Radio");
                        foreach (var value in variablevalue)
                        {
                            document.Form.Fields.Add(radioList);
                            PdfRadioButtonListItem radioItem1 = new PdfRadioButtonListItem(value.ValueCode.ToString());
                            radioItem1.Bounds = new RectangleF(350, result.Bounds.Y, 15, 15);
                            radioList.Items.Add(radioItem1);
                            AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(370, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        }
                        if (variblevaluename?.Count > 0)
                        {
                            int cvalue = variablevalue.FindIndex(x => x.ValueName == variblevaluename.FirstOrDefault().ValueName);
                            radioList.SelectedIndex = cvalue;
                        }
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variable.ProjectDesignVariable.CollectionSource == CollectionSources.MultiCheckBox)
                    {
                        var variablevalue = _context.ProjectDesignVariableValue.Where(b =>
                                                b.ProjectDesignVariableId == variable.ProjectDesignVariable.Id
                                                ).ToList();

                        var variblevaluename = from stvc in _context.ScreeningTemplateValueChild.Where(x =>
                                                  x.DeletedDate == null && x.ScreeningTemplateValueId == variable.Id && x.Value == "true")
                                               join prpjectdesignvalueTemp in _context.ProjectDesignVariableValue.Where(val => val.DeletedDate == null)
                                               on stvc.ProjectDesignVariableValueId equals prpjectdesignvalueTemp.Id into prpjectdesignvalueDto
                                               from prpjectdesignvalue in prpjectdesignvalueDto.DefaultIfEmpty()
                                               select prpjectdesignvalue.ValueName;
                        foreach (var value in variablevalue)
                        {
                            PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, value.ValueCode.ToString());
                            checkField.Bounds = new RectangleF(350, result.Bounds.Y, 10, 10);
                            checkField.Style = PdfCheckBoxStyle.Check;
                            if (variblevaluename.ToList().Contains(value.ValueName))
                                checkField.Checked = true;
                            document.Form.Fields.Add(checkField);
                            AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(370, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        }
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variable.ProjectDesignVariable.CollectionSource == CollectionSources.CheckBox)
                    {
                        var variablevalue = _context.ProjectDesignVariableValue.Where(b =>
                                                  b.ProjectDesignVariableId == variable.ProjectDesignVariable.Id);
                        foreach (var value in variablevalue)
                        {
                            PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, value.ValueCode.ToString());
                            checkField.Bounds = new RectangleF(350, result.Bounds.Y, 10, 10);
                            checkField.Style = PdfCheckBoxStyle.Check;
                            checkField.Checked = true;
                            document.Form.Fields.Add(checkField);
                            AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(370, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            // result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        }
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variable.ProjectDesignVariable.CollectionSource == CollectionSources.Date)
                    {

                        var dt = !string.IsNullOrEmpty(variable.Value) ? DateTime.TryParse(variable.Value, out dDate) ? DateTime.Parse(variable.Value).UtcDateTime().AddMinutes(330).ToString(GeneralSettings.DateFormat) : variable.Value : "";

                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, variable.ProjectDesignVariable.Id.ToString());
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        //textBoxField.ToolTip = "Date Field";
                        textBoxField.Text = dt;
                        document.Form.Fields.Add(textBoxField);


                        AddString(GeneralSettings.DateFormat, result.Page, new Syncfusion.Drawing.RectangleF(460, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variable.ProjectDesignVariable.CollectionSource == CollectionSources.DateTime)
                    {

                        var dttime = !string.IsNullOrEmpty(variable.Value) ? DateTime.TryParse(variable.Value, out dDate) ? DateTime.Parse(variable.Value).UtcDateTime().AddMinutes(330).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variable.Value : "";

                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, variable.ProjectDesignVariable.Id.ToString());
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        //textBoxField.ToolTip = "Date Time";
                        textBoxField.Text = dttime;
                        document.Form.SetDefaultAppearance(true);
                        document.Form.Fields.Add(textBoxField);
                        AddString($"{GeneralSettings.DateFormat} {GeneralSettings.TimeFormat}", result.Page, new Syncfusion.Drawing.RectangleF(460, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variable.ProjectDesignVariable.CollectionSource == CollectionSources.PartialDate)
                    {
                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "PartialDate");
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        //textBoxField.ToolTip = "PartialDate";
                        textBoxField.Text = variable.Value == null ? "" : variable.Value;
                        document.Form.Fields.Add(textBoxField);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variable.ProjectDesignVariable.CollectionSource == CollectionSources.Time)
                    {
                        var time = !string.IsNullOrEmpty(variable.Value) ? DateTime.Parse(variable.Value).UtcDateTime().AddMinutes(330).ToString(GeneralSettings.TimeFormat, CultureInfo.InvariantCulture) : "";

                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "Time");
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        //textBoxField.ToolTip = "Time";
                        textBoxField.Text = time;
                        document.Form.Fields.Add(textBoxField);
                        result = AddString(GeneralSettings.TimeFormat, result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else
                    {
                        result = AddString(variable.ProjectDesignVariable.CollectionSource.ToString(), result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    PdfLayoutResult thirdresult = result;
                    if (secondresult.Bounds.Bottom > thirdresult.Bounds.Bottom)
                    {
                        if (thirdresult.Bounds.Height < secondresult.Bounds.Height)
                            result = AddString("", thirdresult.Page, new Syncfusion.Drawing.RectangleF(0, thirdresult.Bounds.Bottom + 20, thirdresult.Page.GetClientSize().Width, thirdresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        else
                            result = AddString("", secondresult.Page, new Syncfusion.Drawing.RectangleF(0, secondresult.Bounds.Bottom + 20, secondresult.Page.GetClientSize().Width, secondresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    //else
                    //{
                    //    
                    //}
                    ++level2index;
                }
                ++index;
            }
            //document.Template.Top = AddHeader(document, vistitName, vistitName);
            //document.Template.Bottom = AddFooter(document);
            //MemoryStream memoryStream = new MemoryStream();
            //document.Save(memoryStream);
            //return memoryStream;

        }

        private PdfPageTemplateElement VisitTemplateHeader(PdfDocument doc, string vistName, string screeningNO, string subjectNo, string Initial)
        {
            RectangleF rect = new RectangleF(0, 110, doc.Pages[0].GetClientSize().Width, 170);
            PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
            PdfStringFormat stringFormat = new PdfStringFormat();
            stringFormat.MeasureTrailingSpaces = true;
            stringFormat.WordWrap = PdfWordWrapType.Word;
            //Draw title
            header.Graphics.DrawString($"Visit Name :- {vistName}", headerfont, PdfBrushes.Black, new RectangleF(0, 110, header.Width, header.Height), stringFormat);
            header.Graphics.DrawString($"Screening No.:- {screeningNO}", headerfont, PdfBrushes.Black, new RectangleF(0, 130, header.Width, header.Height), stringFormat);
            header.Graphics.DrawString($"Subject No :-{subjectNo}", headerfont, PdfBrushes.Black, new RectangleF(350, 110, header.Width, header.Height), stringFormat);
            header.Graphics.DrawString($"Initial :-{Initial}", headerfont, PdfBrushes.Black, new RectangleF(350, 130, header.Width, header.Height), stringFormat);
            return header;
        }



        public void DataGenerateReport(ReportSettingNew reportSetting, JobMonitoring jobMonitoring)
        {
            var subject = _context.ScreeningEntry.Include(s => s.ScreeningVisit).ThenInclude(s => s.ProjectDesignVisit).Include(x => x.Randomization).Include(x => x.Project)
                .Where(a => reportSetting.SiteId.Contains(a.ProjectId) &&
              (reportSetting.SubjectIds == null || reportSetting.SubjectIds.Select(x => x.Id).ToList().Contains(a.Id))).ToList();

            var base_URL = _uploadSettingRepository.All.OrderByDescending(x => x.Id).FirstOrDefault().DocumentPath;
            FileSaveInfo fileInfo = new FileSaveInfo();
            fileInfo.Base_URL = base_URL;
            fileInfo.ModuleName = Enum.GetName(typeof(JobNameType), jobMonitoring.JobName);
            fileInfo.FolderType = Enum.GetName(typeof(DossierPdfStatus), jobMonitoring.JobDetails);


            var parent = _context.Project.Where(x => x.Id == reportSetting.ProjectId).FirstOrDefault().ProjectCode;
            fileInfo.ParentFolderName = parent + "_" + DateTime.Now.Ticks;
            foreach (var item in subject)
            {
                document = new PdfDocument();
                document.PageSettings.Margins.Top = Convert.ToInt32(reportSetting.TopMargin * 100);
                document.PageSettings.Margins.Bottom = Convert.ToInt32(reportSetting.BottomMargin * 100);
                document.PageSettings.Margins.Left = Convert.ToInt32(reportSetting.LeftMargin * 100);
                document.PageSettings.Margins.Right = Convert.ToInt32(reportSetting.RightMargin * 100);


                PdfSection SectionTOC = document.Sections.Add();
                PdfPage pageTOC = SectionTOC.Pages.Add();

                document.Template.Top = AddHeader(document, item.Project.ProjectCode, Convert.ToBoolean(reportSetting.IsClientLogo), Convert.ToBoolean(reportSetting.IsCompanyLogo));
                PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
                //layoutFormat.Break = PdfLayoutBreakType.FitPage;
                layoutFormat.Layout = PdfLayoutType.Paginate;
                layoutFormat.Break = PdfLayoutBreakType.FitElement;

                RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
                tocresult = new PdfLayoutResult(pageTOC, bounds);

                PdfStringFormat format = new PdfStringFormat();
                format.Alignment = PdfTextAlignment.Left;
                format.WordWrap = PdfWordWrapType.Word;

                var visit = item.ScreeningVisit.Where(x => x.Status != ScreeningVisitStatus.NotStarted).OrderBy(o => o.ProjectDesignVisitId).ThenBy(t => t.RepeatedVisitNumber).ToList();
                DesignVisitData(visit, reportSetting, item.Project.ProjectCode, item);


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
                        graphics.DrawString("Draft", watermarkerfornt, PdfPens.LightBlue, PdfBrushes.LightBlue, new PointF(-150, 500));
                        graphics.Restore();
                    }
                }
                MemoryStream memoryStream = new MemoryStream();
                document.Save(memoryStream);


                //reportSettingNew.TimezoneoffSet = reportSettingNew.TimezoneoffSet * (-1);



                fileInfo.FolderType = Enum.GetName(typeof(DossierPdfStatus), jobMonitoring.JobDetails);
                fileInfo.FileName = item.Randomization.Initial.Replace("/", "") + ".pdf";
                fileInfo.ParentFolderName = fileInfo.ParentFolderName.Trim().Replace(" ", "").Replace("/", "");
                fileInfo.ChildFolderName = item.Project.ProjectCode;

                string fileName = fileInfo.FileName + ".pdf";
                string filePath = string.Empty;
                if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                    filePath = System.IO.Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName, fileName);
                else
                    filePath = System.IO.Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName, fileInfo.ChildFolderName, fileName);

                bool exists = Directory.Exists(filePath);
                if (!exists)
                    if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
                        System.IO.Directory.CreateDirectory(Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName));
                    else
                        System.IO.Directory.CreateDirectory(Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName, fileInfo.ChildFolderName));

                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    memoryStream.WriteTo(fs);
                }
            }
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            // add job Monitor
            jobMonitoring.CompletedTime = DateTime.Now.UtcDateTime();
            jobMonitoring.JobStatus = JobStatusType.Completed;
            jobMonitoring.FolderPath = System.IO.Path.Combine(documentUrl, fileInfo.ModuleName, fileInfo.FolderType);
            jobMonitoring.FolderName = fileInfo.ParentFolderName + ".zip";
            var completeJobMonitoring = _reportBaseRepository.CompleteJobMonitoring(jobMonitoring);


            string Zipfilename = Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, fileInfo.ParentFolderName);
            ZipFile.CreateFromDirectory(Zipfilename, Zipfilename + ".zip");
            Directory.Delete(Zipfilename, true);
        }



    }

}
