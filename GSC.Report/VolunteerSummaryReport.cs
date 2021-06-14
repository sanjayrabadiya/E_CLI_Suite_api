using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Syncfusion.Drawing;
using Microsoft.AspNetCore.Hosting;
using GSC.Respository.Volunteer;
using GSC.Data.Dto.Volunteer;
using AutoMapper;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;
using System.Linq;

namespace GSC.Report
{
    public class VolunteerSummaryReport : IVolunteerSummaryReport
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly IGSCContext _context;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IVolunteerAddressRepository _volunteerAddressRepository;
        public readonly IVolunteerContactRepository _volunteerContactRepository;
        public readonly IVolunteerLanguageRepository _volunteerLanguageRepository;
        public readonly IVolunteerDocumentRepository _volunteerDocumentRepository;
        public readonly IVolunteerBlockHistoryRepository _volunteerBlockHistoryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IMapper _mapper;

        private readonly PdfFont watermarkerfornt = new PdfStandardFont(PdfFontFamily.TimesRoman, 120, PdfFontStyle.Bold);
        private readonly PdfFont largeheaderfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);
        private readonly PdfFont headerfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 14, PdfFontStyle.Bold);
        private readonly PdfFont Regfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Regular);
        private readonly PdfFont smallfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 8);
        private readonly PdfFont regularfont;
        private readonly Stream fontStream;

        private PdfDocument document = null;
        private PdfLayoutResult tocresult = null;
        Dictionary<PdfPageBase, int> pages = new Dictionary<PdfPageBase, int>();
        private List<TocIndexCreate> _pagenumberset = new List<TocIndexCreate>();

        public VolunteerSummaryReport(IHostingEnvironment hostingEnvironment,
            IGSCContext context,
            IAppSettingRepository appSettingRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IVolunteerRepository volunteerRepository,
            IVolunteerAddressRepository volunteerAddressRepository,
            IVolunteerContactRepository volunteerContactRepository,
            IVolunteerLanguageRepository volunteerLanguageRepository,
            IVolunteerDocumentRepository volunteerDocumentRepository,
            IVolunteerBlockHistoryRepository volunteerBlockHistoryRepository,
        IUploadSettingRepository uploadSettingRepository,
            IMapper mapper
        )
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
            _appSettingRepository = appSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _volunteerRepository = volunteerRepository;
            _volunteerAddressRepository = volunteerAddressRepository;
            _volunteerContactRepository = volunteerContactRepository;
            _volunteerLanguageRepository = volunteerLanguageRepository;
            _volunteerDocumentRepository = volunteerDocumentRepository;
            _volunteerBlockHistoryRepository = volunteerBlockHistoryRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _mapper = mapper;
            fontStream = FilePathConvert();
            regularfont = new PdfTrueTypeFont(fontStream, 12);
        }

        private Stream FilePathConvert()
        {
            string path = _hostingEnvironment.WebRootPath + "/fonts/times.ttf";
            byte[] file = File.ReadAllBytes(path);
            Stream stream = new MemoryStream(file);
            return stream;
        }

        public FileStreamResult GetVolunteerSummaryDesign(int VolunteerID)
        {

            VolunteerSearchDto search = new VolunteerSearchDto();
            //search.Id = VoluteerID;
            var volunteer = _volunteerRepository.GetVolunteerDetail(false, VolunteerID);

            //Create a new PDF document
            PdfDocument doc = new PdfDocument();
            //Add a page
            PdfPage page = doc.Pages.Add();

            PdfGraphics graphics = page.Graphics;

            //Set the standard font.

            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);

            //Draw the text.

            //graphics.DrawString(volunteer[0].FullName, font, PdfBrushes.Black, new PointF(0, 0));
            doc.Pages[0].Graphics.DrawString(volunteer[0].FullName, font, new PdfSolidBrush(new PdfColor(213, 123, 19)), 0, 0);

            doc.Pages[0].Graphics.DrawString("Code", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 0, 40);
            doc.Pages[0].Graphics.DrawString("Old Reference Number", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 100, 40);
            doc.Pages[0].Graphics.DrawString("Registration Date", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 280, 40);

            doc.Pages[0].Graphics.DrawString(volunteer[0].VolunteerNo, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 0, 60);
            doc.Pages[0].Graphics.DrawString(volunteer[0].RefNo, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 100, 60);
            doc.Pages[0].Graphics.DrawString(volunteer[0].RegisterDate.ToString(), Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 280, 60);

            doc.Pages[0].Graphics.DrawString("Abbreviation", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 0, 80);
            doc.Pages[0].Graphics.DrawString("Date Of Birth", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 100, 80);
            doc.Pages[0].Graphics.DrawString("Age", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 280, 80);

            doc.Pages[0].Graphics.DrawString(volunteer[0].AliasName, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 0, 100);
            doc.Pages[0].Graphics.DrawString(volunteer[0].DateOfBirth.ToString(), Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 100, 100);
            doc.Pages[0].Graphics.DrawString(volunteer[0].FromAge.ToString(), Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 280, 100);


            doc.Pages[0].Graphics.DrawString("Religion", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 0, 120);
            doc.Pages[0].Graphics.DrawString("Occupation", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 100, 120);
            doc.Pages[0].Graphics.DrawString("Educaiton Qualification", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 280, 120);

            doc.Pages[0].Graphics.DrawString(volunteer[0].Religion, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 0, 140);
            doc.Pages[0].Graphics.DrawString(volunteer[0].Occupation, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 100, 140);
            doc.Pages[0].Graphics.DrawString(volunteer[0].Education, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 280, 140);

            doc.Pages[0].Graphics.DrawString("Race", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 0, 160);
            doc.Pages[0].Graphics.DrawString("Marital Status", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 100, 160);
            doc.Pages[0].Graphics.DrawString("Type Of Population", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 280, 160);

            doc.Pages[0].Graphics.DrawString(volunteer[0].Race, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 0, 180);
            doc.Pages[0].Graphics.DrawString(volunteer[0].MaritalStatus, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 100, 180);
            doc.Pages[0].Graphics.DrawString(volunteer[0].PopulationType, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 280, 180);

            doc.Pages[0].Graphics.DrawString("Gender", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 0, 200);
            doc.Pages[0].Graphics.DrawString("Annual Income", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 100, 200);
            doc.Pages[0].Graphics.DrawString("Food Type", headerfont, new PdfSolidBrush(new PdfColor(124, 143, 166)), 280, 200);

            doc.Pages[0].Graphics.DrawString(volunteer[0].Gender, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 0, 220);
            doc.Pages[0].Graphics.DrawString(volunteer[0].AnnualIncome.ToString(), Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 100, 220);
            doc.Pages[0].Graphics.DrawString(volunteer[0].FoodType, Regfont, new PdfSolidBrush(new PdfColor(0, 0, 0)), 280, 220);



            PdfGridCellStyle cellStyle = new PdfGridCellStyle();
            cellStyle.Borders.All = PdfPens.White;
            PdfGridRow header;
            
            //Creates the header style
            PdfGridCellStyle headerStyle = new PdfGridCellStyle();
            headerStyle.Borders.All = new PdfPen(new PdfColor(5, 159, 223));
            headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(5, 159, 223));
            headerStyle.TextBrush = PdfBrushes.White;
            headerStyle.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12f, PdfFontStyle.Regular);

            

            //Applies the header style
            
            cellStyle.Borders.Bottom = new PdfPen(new PdfColor(217, 217, 217), 0.70f);
            cellStyle.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12f);
            cellStyle.TextBrush = new PdfSolidBrush(new PdfColor(131, 130, 136));
            //Creates the layout format for grid


            //Create a PdfGrid
            PdfGrid pdfGridAddress = new PdfGrid();
            //Assign data source
            pdfGridAddress.DataSource = DesginVoluteerAddress(VolunteerID);
            //Add layout format for grid pagination
            PdfGridLayoutFormat layoutFormat = new PdfGridLayoutFormat();
            layoutFormat.Layout = PdfLayoutType.Paginate;
            //Draw grid to the page of PDF document
            header = pdfGridAddress.Headers[0];
            //Adds cell customizations
            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            }
            header.ApplyStyle(headerStyle);
            PdfLayoutResult result = pdfGridAddress.Draw(page, new PointF(0, 800), layoutFormat);


            //Create a PdfGrid
            PdfGrid pdfContact = new PdfGrid();            
            //Assign data source
            pdfContact.DataSource = DesginVoluteerContact(VolunteerID);
            header = pdfContact.Headers[0];
            //Adds cell customizations
            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            }
            header.ApplyStyle(headerStyle);
            //Draw grid to the resultant page of the first grid
            result = pdfContact.Draw(result.Page, new PointF(10, result.Bounds.Height + 90));


            //Create a PdfGrid
            PdfGrid pdfLanguage = new PdfGrid();
            //Assign data source
            pdfLanguage.DataSource = DesginVoluteerLanguage(VolunteerID);
            header = pdfLanguage.Headers[0];
            //Adds cell customizations
            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            }
            header.ApplyStyle(headerStyle);
            //Draw grid to the resultant page of the first grid
            result = pdfLanguage.Draw(result.Page, new PointF(10, result.Bounds.Height + 170));


            //Create a PdfGrid
            PdfGrid pdfVolDocumnet = new PdfGrid();
            //Assign data source
            pdfVolDocumnet.DataSource = DesginVoluteerDocument(VolunteerID);
            header = pdfVolDocumnet.Headers[0];
            //Adds cell customizations
            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            }
            header.ApplyStyle(headerStyle);
            //Draw grid to the resultant page of the first grid
            result =  pdfVolDocumnet.Draw(result.Page, new PointF(10, result.Bounds.Height + 230));


            //Save and the document
            MemoryStream memoryStream = new MemoryStream();
            doc.Save(memoryStream);
            memoryStream.Position = 0;
            FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/pdf");
            fileStreamResult.FileDownloadName = "blankreport.pdf";
            return fileStreamResult;
        }


        public FileStreamResult GetVolunteerSummaryDesign1(int VoluteerID)
        {
            //var projectdetails = _projectDesignRepository.FindByInclude(i => i.ProjectId == reportSetting.ProjectId, i => i.Project).SingleOrDefault();
            //var projectDesignvisit = _projectDesignVisitRepository.GetVisitsByProjectDesignId(projectdetails.Id);

            //var projectDetails = _reportBaseRepository.GetBlankPdfData(reportSetting);
            VolunteerSearchDto search = new VolunteerSearchDto();
            //search.Id = VoluteerID;
            var volunteer = _volunteerRepository.GetVolunteerDetail(false, VoluteerID);
            //var vol = _mapper.Map<VolunteerDto>(volunteer);


            document = new PdfDocument();
            //document.PageSettings.Margins.Top = Convert.ToInt32(reportSetting.TopMargin * 100);
            //document.PageSettings.Margins.Bottom = Convert.ToInt32(reportSetting.BottomMargin * 100);
            //document.PageSettings.Margins.Left = Convert.ToInt32(reportSetting.LeftMargin * 100);
            //document.PageSettings.Margins.Right = Convert.ToInt32(reportSetting.RightMargin * 100);

            PdfSection SectionTOC = document.Sections.Add();
            PdfPage pageTOC = SectionTOC.Pages.Add();

            document = new PdfDocument();
            //Add a page
            //PdfPage page = document.Pages.Add();

            ////Create Pdf graphics for the page
            //PdfGraphics g = page.Graphics;

            ////Create a solid brush
            //PdfBrush brush = new PdfSolidBrush(Color.Black);

            ////Set the font
            //PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 36);




            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            //layoutFormat.Break = PdfLayoutBreakType.FitPage;
            layoutFormat.Layout = PdfLayoutType.Paginate;
            layoutFormat.Break = PdfLayoutBreakType.FitElement;

            //Create a solid brush
            PdfBrush brush = new PdfSolidBrush(Color.Black);

            //Set the font
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 18);

            //Create Pdf graphics for the page

            RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            tocresult = new PdfLayoutResult(pageTOC, bounds);

            PdfStringFormat headformat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Top);
            PdfTextElement indexheader = new PdfTextElement("Volunteer Summary", largeheaderfont, PdfBrushes.Black);
            indexheader.StringFormat = headformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            //RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            //tocresult = new PdfLayoutResult(pageTOC, bounds);

            PdfStringFormat tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            PdfTextElement indexTitle = new PdfTextElement(volunteer[0].FullName, largeheaderfont, PdfBrushes.Black);
            indexTitle.StringFormat = tocformat;
            tocresult = indexTitle.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.WordWrap = PdfWordWrapType.Word;


            //DesignVolunteerProfile(volunteer);
            DesginVoluteerAddress(VoluteerID);
            DesginVoluteerContact(VoluteerID);
            DesginVoluteerLanguage(VoluteerID);
            DesginVoluteerDocument(VoluteerID);


            SetPageNumber();
            MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Position = 0;
            FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/pdf");
            fileStreamResult.FileDownloadName = "blankreport.pdf";
            return fileStreamResult;

        }

        private void DesignVolunteerProfile(IList<VolunteerGridDto> volunteer)
        {
            PdfSection SectionTOC = document.Sections.Add();
            PdfPage pageTOC = SectionTOC.Pages.Add();

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            //layoutFormat.Break = PdfLayoutBreakType.FitPage;
            layoutFormat.Layout = PdfLayoutType.Paginate;
            layoutFormat.Break = PdfLayoutBreakType.FitElement;

            //Create a solid brush
            PdfBrush brush = new PdfSolidBrush(Color.Black);

            //Set the font
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 18);

            //Create Pdf graphics for the page

            RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            tocresult = new PdfLayoutResult(pageTOC, bounds);

            PdfStringFormat headformat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Top);
            PdfTextElement indexheader = new PdfTextElement("Volunteer Summary", largeheaderfont, PdfBrushes.Black);
            indexheader.StringFormat = headformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            //RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            //tocresult = new PdfLayoutResult(pageTOC, bounds);

            PdfStringFormat tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            PdfTextElement indexTitle = new PdfTextElement(volunteer[0].FullName, largeheaderfont, PdfBrushes.Black);
            indexTitle.StringFormat = tocformat;
            tocresult = indexTitle.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.WordWrap = PdfWordWrapType.Word;


        }

        private DataTable DesginVoluteerAddress(int VolunteerID)
        {
            var volunteerAddress = _volunteerAddressRepository.GetAddresses(VolunteerID);


            //Creates the datasource for the table
            DataTable addressDetails = new DataTable();

            addressDetails.Columns.Add("Address");
            addressDetails.Columns.Add("City");
            addressDetails.Columns.Add("State");
            addressDetails.Columns.Add("Country");
            addressDetails.Columns.Add("Zip/Post Code");
            addressDetails.Columns.Add("Current Address?");
            addressDetails.Columns.Add("Parmenent Address?");


            if (volunteerAddress.Count > 0)
            {
                foreach (var address in volunteerAddress)
                {

                    addressDetails.Rows.Add(new object[] {
                                                      address.Location.Address
                                                    , address.Location.CityName
                                                    , address.Location.StateName
                                                    , address.Location.CountryName
                                                    , address.Location.Zip
                                                    , address.IsCurrent ? "YES" : "NO"
                                                    , address.IsPermanent ? "YES":"NO"
                                                });
                }
            }
            return addressDetails;
        }

        private DataTable DesginVoluteerContact(int VolunteerID)
        {
            var volunteercontact = _volunteerContactRepository.GetContactTypeList(VolunteerID);


            //Creates the datasource for the table
            DataTable contactDetails = new DataTable();

            contactDetails.Columns.Add("Contact");
            contactDetails.Columns.Add("Contact No.");
            contactDetails.Columns.Add("Contact Name");
            contactDetails.Columns.Add("Default Contact?");
            contactDetails.Columns.Add("Emergency Contact?");


            if (volunteercontact.Count > 0)
            {
                foreach (var contact in volunteercontact)
                {

                    contactDetails.Rows.Add(new object[] {
                                                      contact.ContactTypeName
                                                    , contact.ContactNo
                                                    , contact.ContactName
                                                    , contact.IsDefault ? "YES" : "NO"
                                                    , contact.IsEmergency ? "YES":"NO"
                                                });
                }
            }

            return contactDetails;
        }

        private DataTable DesginVoluteerLanguage(int VolunteerID)
        {
            var volunteerLanguage = _volunteerLanguageRepository.GetLanguages(VolunteerID, false);

            //Creates the datasource for the table
            DataTable languageDetails = new DataTable();

            languageDetails.Columns.Add("Language Name");
            languageDetails.Columns.Add("Note");
            languageDetails.Columns.Add("Read?");
            languageDetails.Columns.Add("Write?");
            languageDetails.Columns.Add("Speak?");

            if (volunteerLanguage.Count > 0)
            {
                foreach (var language in volunteerLanguage)
                {

                    languageDetails.Rows.Add(new object[] {
                                                      language.LanguageName
                                                    , language.Note
                                                    , language.IsRead ? "YES" : "NO"
                                                    , language.IsWrite ? "YES":"NO"
                                                    , language.IsSpeak ? "YES":"NO"
                                                });
                }
            }

            return languageDetails;
        }

        private DataTable DesginVoluteerDocument(int VolunteerID)
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var volunteerDocument = _volunteerDocumentRepository
                .FindByInclude(t => t.VolunteerId == VolunteerID && t.DeletedDate == null, t => t.DocumentType,
                    t => t.DocumentName).OrderByDescending(x => x.Id).ToList();
            volunteerDocument.ForEach(t => t.PathName = documentUrl + t.PathName);


            //Creates the datasource for the table
            DataTable documentDetails = new DataTable();

            documentDetails.Columns.Add("Document Type");
            documentDetails.Columns.Add("File Name");
            documentDetails.Columns.Add("Remark");


            if (volunteerDocument.Count > 0)
            {
                foreach (var document in volunteerDocument)
                {

                    documentDetails.Rows.Add(new object[] {
                                                      document.DocumentType.TypeName
                                                    , document.FileName
                                                    , document.Note
                                                });
                }
            }

            return documentDetails;
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

    }
}
