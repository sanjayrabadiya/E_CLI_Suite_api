using BoldReports.Web;
using BoldReports.Writer;
using GSC.Data.Dto.Common;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Report;
using GSC.Data.Entities.Report;
using GSC.Helper;
using GSC.Report.Common;
using GSC.Respository.Client;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Shared.Extension;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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


        //private PdfFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
        private readonly PdfFont watermarkerfornt = new PdfStandardFont(PdfFontFamily.TimesRoman, 120, PdfFontStyle.Bold);
        private readonly PdfFont largeheaderfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);
        private readonly PdfFont headerfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 14, PdfFontStyle.Bold);
        private readonly PdfFont regularfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
        private readonly PdfFont smallfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 8);

        // private float yPosition { get; set; }

        public ReportSyncfusion(IHostingEnvironment hostingEnvironment, IProjectDesignRepository projectDesignRepository, IProjectDesignVisitRepository projectDesignVisitRepository,
        IProjectDesignTemplateRepository projectDesignTemplateRepository, IProjectDesignVariableRepository projectDesignVariableRepository, IUploadSettingRepository uploadSettingRepository, IReportBaseRepository reportBaseRepository, ICompanyRepository companyRepository, IClientRepository clientRepository)
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
        }

        public void BlankReportGenerate(ReportSettingNew reportSetting, JobMonitoring jobMonitoring)
        {
            var projectdetails = _projectDesignRepository.FindByInclude(i => i.ProjectId == reportSetting.ProjectId, i => i.Project).SingleOrDefault();
            var projectDesignvisit = _projectDesignVisitRepository.GetVisitsByProjectDesignId(projectdetails.Id);

            PdfDocument document = new PdfDocument();
            document.PageSettings.Margins.Left = 50;
            document.PageSettings.Margins.Right = 50;
            //document.PageSettings.Margins.Top = 200;
            document.PageSettings.Margins.Bottom = 100;

            var designvisit = DesignVisit(projectDesignvisit, reportSetting);

            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(designvisit, true);

            //Disable the incremental update
            loadedDocument.FileStructure.IncrementalUpdate = false;
            //Set the compression level
            loadedDocument.Compression = PdfCompressionLevel.Best;

            //margin
            //document.PageSettings.Margins.Left = 0;
            //document.PageSettings.Margins.Right = 0;
            //document.PageSettings.Margins.Top = 0;
            //document.PageSettings.Margins.Bottom = 0;
            //for (int i = 0; i < loadedDocument.Pages.Count; i++)
            //{
            //    //Get loaded page as template
            //    PdfTemplate template = loadedDocument.Pages[i].CreateTemplate();
            //    //Create new page
            //    PdfPage page = document.Pages.Add();
            //    //Create Pdf graphics for the page
            //    PdfGraphics g = page.Graphics;
            //    //Draw template with the size as loaded page size
            //    g.DrawPdfTemplate(template, new PointF(0, 0), new SizeF(page.GetClientSize().Width, page.GetClientSize().Height));
            //}
            //end margin

            document.ImportPageRange(loadedDocument, 0, loadedDocument.Pages.Count - 1);

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
                    graphics.DrawString("Draft", watermarkerfornt, PdfPens.LightBlue, PdfBrushes.LightBlue, new PointF(-250, 500));
                    graphics.Restore();
                }
            }

            document.Template.Top = AddHeader(document, projectdetails.Project.ProjectCode, Convert.ToBoolean(reportSetting.IsClientLogo), Convert.ToBoolean(reportSetting.IsCompanyLogo));
            //document.Template.Bottom = AddFooter(document);

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



            // return fileStreamResult;
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
            //brush = new PdfSolidBrush(Color.Gray);
            //font = new PdfStandardFont(PdfFontFamily.Helvetica, 6, PdfFontStyle.Bold);

            format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.LineAlignment = PdfVerticalAlignment.Bottom;

            //Draw description
            // header.Graphics.DrawString(description, font, brush, new RectangleF(0, 0, header.Width, header.Height - 8), format);

            //Draw some lines in the header
            //pen = new PdfPen(Color.DarkBlue, 0.7f);
            //header.Graphics.DrawLine(pen, 0, 0, header.Width, 0);
            //pen = new PdfPen(Color.DarkBlue, 2f);
            //header.Graphics.DrawLine(pen, 0, 03, header.Width + 3, 03);
            pen = new PdfPen(Color.Black, 2f);
            // header.Graphics.DrawLine(pen, 0, header.Height - 3, header.Width, header.Height - 3);
            header.Graphics.DrawLine(pen, 0, header.Height, header.Width, header.Height);

            return header;
        }

        private PdfPageTemplateElement AddFooter(PdfDocument doc)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

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
        private MemoryStream DesignVisit(IList<DropDownDto> designvisit, ReportSettingNew reportSetting)
        {

            PdfDocument document = new PdfDocument();
            //PdfPage page = document.Pages.Add();
            //RectangleF bounds = new RectangleF(new PointF(0, 0), new SizeF(0, 0));
            //PdfLayoutResult result = new PdfLayoutResult(page, bounds);

            //document.PageSettings.Margins.Left = 50;
            //document.PageSettings.Margins.Right = 50;
            // document.PageSettings.Margins.Top = 100;
            //document.PageSettings.Margins.Bottom = 100;


            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.WordWrap = PdfWordWrapType.Word;


            foreach (var template in designvisit)
            {
                var projecttemplate = _projectDesignTemplateRepository.GetTemplateDropDown(template.Id);
                var designtemplate = DesignTemplate(projecttemplate, reportSetting);
                PdfDocument subdocument = new PdfDocument();

                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(designtemplate, true);
                // document.Append(loadedDocument);
                subdocument.ImportPageRange(loadedDocument, 0, loadedDocument.Pages.Count - 1);
                //document.Template.Top = VisitTemplateHeader(document, "Hello", "1234");
                // document.Template.Top = AddHeader(document);   

                foreach (PdfPage page1 in subdocument.Pages)
                {
                    //Draw description
                    // page.Graphics.DrawString("Clinvigilant", font, PdfBrushes.Black, new RectangleF(0, 0, page.GetClientSize().Width, page.GetClientSize().Height), format);
                    // page.Section.Pages.Add();

                    page1.Graphics.DrawString($"Visit Name :-", headerfont, PdfBrushes.Black, new PointF(0, 120));
                    page1.Graphics.DrawString($"{template.Value}", regularfont, PdfBrushes.Black, new PointF(100, 121), format);

                    page1.Graphics.DrawString("Subject No :-", headerfont, PdfBrushes.Black, new PointF(350, 120));

                    page1.Graphics.DrawString("Screening No. :-", headerfont, PdfBrushes.Black, new PointF(0, 135));
                    page1.Graphics.DrawString("Initial :-", headerfont, PdfBrushes.Black, new PointF(350, 135));

                }

                MemoryStream submemoryStream = new MemoryStream();
                document.Save(submemoryStream);
                subdocument.Save(submemoryStream);

                PdfLoadedDocument sloadedDocument = new PdfLoadedDocument(submemoryStream, true);
                //document.Append(loadedDocument);
                document.ImportPageRange(sloadedDocument, 0, sloadedDocument.Pages.Count - 1);
            }

            MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream);

            return memoryStream;

        }


        private MemoryStream DesignTemplate(IList<DropDownDto> designtemplate, ReportSettingNew reportSetting)
        {
            PdfDocument document = new PdfDocument();
            document.PageSettings.Margins.Left = 0;
            document.PageSettings.Margins.Right = 0;
            document.PageSettings.Margins.Top = 170;
            document.PageSettings.Margins.Bottom = 0;

            //document.PageSettings.Size = PdfPageSize.A4;
            PdfPage page = document.Pages.Add();
            RectangleF bounds = new RectangleF(new PointF(0, 0), new SizeF(0, 0));
            PdfLayoutResult result = new PdfLayoutResult(page, bounds);
            int index = 1;

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            layoutFormat.Break = PdfLayoutBreakType.FitPage;
            layoutFormat.Layout = PdfLayoutType.Paginate;




            foreach (var designt in designtemplate)
            {
                result = AddString($"{index}.{designt.Value}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, largeheaderfont, layoutFormat);

                AddString("Sr# ", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);
                AddString("Question", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Bottom + 20, 100, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);
                result = AddString("Answers", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Bottom + 20, 100, result.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);

                PdfPen pen = new PdfPen(Color.Gray, 1f);
                result.Page.Graphics.DrawLine(pen, 0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Bounds.Y + 20);

                result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                var variabledetails = _projectDesignVariableRepository.GetVariabeAnnotationDropDownForProjectDesign(designt.Id);
                int level2index = 1;
                foreach (var variable in variabledetails)
                {
                    result = AddString($"{index}.{level2index}", result.Page, new Syncfusion.Drawing.RectangleF(0, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    // result = AddString(variable.Value, result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                    var variabled = _projectDesignVariableRepository.FindByInclude(t => t.Id == variable.Id, t => t.Values, t => t.Remarks, t => t.Unit).FirstOrDefault();

                    string annotation = String.IsNullOrEmpty(variabled.Annotation) ? "" : $"[{variabled.Annotation}]";
                    string CollectionAnnotation = String.IsNullOrEmpty(variabled.CollectionAnnotation) ? "" : $"({variabled.CollectionAnnotation})";
                    if (reportSetting.AnnotationType == true)
                        result = AddString($"{variable.Value}\n {annotation}   {CollectionAnnotation}", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    else
                        result = AddString($"{variable.Value}", result.Page, new Syncfusion.Drawing.RectangleF(50, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);


                    PdfLayoutResult secondresult = result;

                    if (variabled.Unit != null)
                        AddString(variabled.Unit.UnitName, result.Page, new Syncfusion.Drawing.RectangleF(460, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                    if (variabled.CollectionSource == CollectionSources.TextBox)
                    {
                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "FirstName");
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        textBoxField.ToolTip = "First Name";
                        document.Form.Fields.Add(textBoxField);
                       // result = AddString(str, result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Bottom, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.MultilineTextBox)
                    {
                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "multipletextbox");
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 500, 20);
                        textBoxField.ToolTip = "Multi line";
                        textBoxField.Multiline = true;
                        document.Form.Fields.Add(textBoxField);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                    }
                    else if (variabled.CollectionSource == CollectionSources.ComboBox)
                    {
                        PdfComboBoxField comboBox = new PdfComboBoxField(result.Page, "JobTitle");
                        comboBox.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        comboBox.BorderColor = new PdfColor(Color.Gray);
                        comboBox.ToolTip = "Job Title";
                        foreach (var value in variabled.Values)
                        {
                            comboBox.Items.Add(new PdfListFieldItem(value.ValueName, value.ValueName));
                        }
                        document.Form.Fields.Add(comboBox);
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
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.MultiCheckBox)
                    {
                        foreach (var value in variabled.Values)
                        {
                            PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, "UG");
                            checkField.Bounds = new RectangleF(350, result.Bounds.Y, 10, 10);
                            checkField.Style = PdfCheckBoxStyle.Check;
                            document.Form.Fields.Add(checkField);
                            AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(370, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        }
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Bottom, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.CheckBox)
                    {
                        foreach (var value in variabled.Values)
                        {
                            PdfCheckBoxField checkField = new PdfCheckBoxField(result.Page, "singlecheckbox");
                            checkField.Bounds = new RectangleF(350, result.Bounds.Y, 10, 10);
                            checkField.Style = PdfCheckBoxStyle.Check;
                            document.Form.Fields.Add(checkField);
                            AddString(value.ValueName, result.Page, new Syncfusion.Drawing.RectangleF(370, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                            // result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                        }
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.Date)
                    {
                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "DateField");
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        textBoxField.ToolTip = "Date Field";
                        document.Form.Fields.Add(textBoxField);
                        AddString("MM/dd/yyyy", result.Page, new Syncfusion.Drawing.RectangleF(460, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.DateTime)
                    {
                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "DatetimeField");
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        textBoxField.ToolTip = "Date Time";
                        document.Form.Fields.Add(textBoxField);
                        AddString("", result.Page, new Syncfusion.Drawing.RectangleF(460, result.Bounds.Y, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, smallfont, layoutFormat);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.PartialDate)
                    {
                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "PartialDate");
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        textBoxField.ToolTip = "PartialDate";
                        document.Form.Fields.Add(textBoxField);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else if (variabled.CollectionSource == CollectionSources.Time)
                    {
                        PdfTextBoxField textBoxField = new PdfTextBoxField(result.Page, "Time");
                        textBoxField.Bounds = new RectangleF(350, result.Bounds.Y, 100, 20);
                        textBoxField.ToolTip = "Time";
                        document.Form.Fields.Add(textBoxField);
                        result = AddString("", result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y + 20, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                    }
                    else
                    {
                        result = AddString(variabled.CollectionSource.ToString(), result.Page, new Syncfusion.Drawing.RectangleF(350, result.Bounds.Y, 200, result.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

                        //textElement = new PdfTextElement("[Hello]", new PdfStandardFont(PdfFontFamily.TimesRoman, 12));
                        //result = textElement.Draw(result.Page, new Syncfusion.Drawing.RectangleF(450, result.Bounds.Y, 100, result.Page.GetClientSize().Height), layoutFormat);

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
            //document.Template.Top = AddHeader(document);
            //document.Template.Bottom = AddFooter(document);
            MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream);

            //PdfLoadedDocument loadedDocument = new PdfLoadedDocument(memoryStream);
            //document.ImportPageRange(loadedDocument, 0, loadedDocument.Pages.Count - 1);
            //document.Template.Top = AddHeader(document);
            //memoryStream = new MemoryStream();
            //document.Save(memoryStream);

            return memoryStream;

        }
        private PdfLayoutResult AddString(string note, PdfPage page, RectangleF position, PdfBrush brush, PdfFont font, PdfLayoutFormat pdfLayoutFormat)
        {
            PdfTextElement richTextElement = new PdfTextElement(note, font, brush);
            //Draws String       
            PdfStringFormat stringFormat = new PdfStringFormat();
            stringFormat.MeasureTrailingSpaces = true;
            stringFormat.WordWrap = PdfWordWrapType.Word;
            richTextElement.StringFormat = stringFormat;
            PdfLayoutResult result = richTextElement.Draw(page, position);
            return result;
        }

    }

}
