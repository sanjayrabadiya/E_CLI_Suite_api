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
using GSC.Respository.Client;

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
        private readonly ICompanyRepository _companyRepository;
        private readonly IClientRepository _clientRepository;

        private readonly PdfFont watermarkerfornt = new PdfStandardFont(PdfFontFamily.TimesRoman, 120, PdfFontStyle.Bold);
        private readonly PdfFont extralargeheaderfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 20, PdfFontStyle.Bold);
        private readonly PdfFont largeheaderfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);
        private readonly PdfFont headerfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 14, PdfFontStyle.Bold);
        private readonly PdfFont regularfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
        private readonly PdfFont smallfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 8);
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
        ICompanyRepository companyRepository,
        IClientRepository clientRepository,
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
            _companyRepository = companyRepository;
            _clientRepository = clientRepository;
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
            var volunteer = _volunteerRepository.GetVolunteerDetail(false, VolunteerID);


            //Create a new PDF document
            PdfDocument doc = new PdfDocument();

            //Add a page
            PdfPage page = doc.Pages.Add();

            doc.PageSettings.Margins.Top = Convert.ToInt32(0.5 * 100);
            //doc.PageSettings.Margins.Bottom = Convert.ToInt32(0.5 * 100);
            doc.PageSettings.Margins.Left = Convert.ToInt32(0.5 * 100);
            doc.PageSettings.Margins.Right = Convert.ToInt32(0.5 * 100);

            PdfGraphics graphics = page.Graphics;

            doc.Template.Top = AddHeader(doc);
            doc.Template.Bottom = AddFooter(doc);

            PdfSolidBrush brush = new PdfSolidBrush(new PdfColor(0, 0, 0));
            RectangleF bounds = new RectangleF(page.GetClientSize().Width - 75, 0, 65f, 65f);
            PdfPen pens = new PdfPen(Color.Black);

            //Add a Volunteer Profile Image

            SizeF imageSize = new SizeF(65f, 65f);
            var imagePath = _uploadSettingRepository.GetImagePath();
            if (File.Exists($"{imagePath}/{volunteer[0].ProfilePic}"))
            {
                FileStream VoluntreerProfileinputstream = new FileStream($"{imagePath}/{volunteer[0].ProfilePic}", FileMode.Open, FileAccess.Read);
                PdfImage img = new PdfBitmap(VoluntreerProfileinputstream);
                var VoluntreerProfile = new PointF(page.GetClientSize().Width - 75, 0);
                page.Graphics.DrawImage(img, VoluntreerProfile, imageSize);
                page.Graphics.DrawRectangle(pens, new RectangleF(page.GetClientSize().Width - 75, 0, 65f, 65f));

            }

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            //layoutFormat.Break = PdfLayoutBreakType.FitPage;
            layoutFormat.Layout = PdfLayoutType.Paginate;
            //not fit page then next page
            layoutFormat.Break = PdfLayoutBreakType.FitElement;

            tocresult = new PdfLayoutResult(page, bounds);

            PdfStringFormat tocformat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Top);
            PdfTextElement indexheader = new PdfTextElement();

            tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            indexheader = new PdfTextElement(volunteer[0].FullName, headerfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y + 80, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            tocformat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Top);
            indexheader = new PdfTextElement(volunteer[0].VolunteerNo, headerfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(tocresult.Bounds.X, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            tocformat = new PdfStringFormat(PdfTextAlignment.Right, PdfVerticalAlignment.Top);
            indexheader = new PdfTextElement(volunteer[0].AliasName, headerfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(tocresult.Bounds.X, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            //Draw Rectangle For Profile Details
            brush = new PdfSolidBrush(new PdfColor(255, 255, 255));
            bounds = new RectangleF(0, 100, page.GetClientSize().Width, 270);
            page.Graphics.DrawRectangle(PdfPens.Black, brush, bounds);

            tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);

            indexheader = new PdfTextElement("Old Reference Number", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 30, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].RefNo, regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            indexheader = new PdfTextElement("Registration Date", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(Convert.ToDateTime(volunteer[0].RegisterDate).ToString("dd-MM-yyyy"), regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Date Of Birth", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(Convert.ToDateTime(volunteer[0].DateOfBirth).ToString("dd-MM-yyyy"), regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Age", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].FromAge.ToString(), regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Religion", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].Religion, regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Occupation", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].Occupation, regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Educaiton Qualification", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].Education, regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Race", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].Race, regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Marital Status", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].MaritalStatus, regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Type Of Population", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].PopulationType, regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Gender", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].Gender, regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Annual Income", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].AnnualIncome.ToString(), regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            indexheader = new PdfTextElement("Food Type", regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Y + 20, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            indexheader = new PdfTextElement(volunteer[0].FoodType, regularfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            indexheader = new PdfTextElement("Address Details", headerfont, PdfBrushes.Black);
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y + 40, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            PdfGridRow header;

            //Creates the header style
            PdfGridCellStyle headerStyle = new PdfGridCellStyle();
            headerStyle.Borders.All = new PdfPen(new PdfColor(5, 159, 223));
            headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(5, 159, 223));
            headerStyle.TextBrush = PdfBrushes.Black;
            headerStyle.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12f, PdfFontStyle.Regular);


            /* Address Detail Grid */
            PdfGrid pdfGridAddress = new PdfGrid();
            //Assign data source
            pdfGridAddress.DataSource = DesginVoluteerAddress(VolunteerID);
            //Add layout format for grid pagination
            layoutFormat = new PdfGridLayoutFormat();
            layoutFormat.Layout = PdfLayoutType.Paginate;
            //layoutFormat.Break = PdfLayoutBreakType.FitPage;
            //Draw grid to the page of PDF document
            header = pdfGridAddress.Headers[0];
            //Adds cell customizations
            for (int i = 0; i < pdfGridAddress.Columns.Count; i++)
            {
                pdfGridAddress.Columns[i].Format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);

            }
            header.ApplyStyle(headerStyle);
            tocresult = pdfGridAddress.Draw(page, new PointF(0, tocresult.Bounds.Bottom+ 10), layoutFormat);


            /* Contact Detail Grid */
            tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            indexheader = new PdfTextElement("Contact Details", headerfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            //Create a PdfGrid
            PdfGrid pdfContact = new PdfGrid();
            //Assign data source
            pdfContact.DataSource = DesginVoluteerContact(VolunteerID);
            header = pdfContact.Headers[0];
            //Adds cell customizations
            header.ApplyStyle(headerStyle);
            for (int i = 0; i < pdfContact.Columns.Count; i++)
            {
                pdfContact.Columns[i].Format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);

            }
            //Draw grid to the resultant page of the first grid
            tocresult = pdfContact.Draw(tocresult.Page, new PointF(0, tocresult.Bounds.Bottom + 10));

            /* Language Detail Grid */
            tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            indexheader = new PdfTextElement("Language Details", headerfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);


            //Create a PdfGrid
            PdfGrid pdfLanguage = new PdfGrid();
            //Assign data source
            pdfLanguage.DataSource = DesginVoluteerLanguage(VolunteerID);
            header = pdfLanguage.Headers[0];
            //Adds cell customizations
            header.ApplyStyle(headerStyle);
            for (int i = 0; i < pdfLanguage.Columns.Count; i++)
            {
                pdfLanguage.Columns[i].Format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);

            }
            //Draw grid to the resultant page of the first grid
            tocresult = pdfLanguage.Draw(tocresult.Page, new PointF(0, tocresult.Bounds.Y + 20));


            /* Documnet Detail Grid */
            tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            indexheader = new PdfTextElement("Document Details", headerfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            ////Create a PdfGrid
            PdfGrid pdfVolDocumnet = new PdfGrid();
            //Assign data source
            pdfVolDocumnet.DataSource = DesginVoluteerDocument(VolunteerID);
            header = pdfVolDocumnet.Headers[0];
            header.ApplyStyle(headerStyle);
            for (int i = 0; i < pdfVolDocumnet.Columns.Count; i++)
            {
                pdfVolDocumnet.Columns[i].Format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);

            }
            //Draw grid to the resultant page of the first grid
            tocresult = pdfVolDocumnet.Draw(tocresult.Page, new PointF(0, tocresult.Bounds.Bottom + 10));


            //Save and the document
            MemoryStream memoryStream = new MemoryStream();
            doc.Save(memoryStream);
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

        private PdfPageTemplateElement AddHeader(PdfDocument doc)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 70);
            PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 24);
            Color activeColor = Color.FromArgb(44, 71, 120);
            SizeF imageSize = new SizeF(50f, 50f);

            //var imagePath = _uploadSettingRepository.GetImagePath();
            //var companydetail = _companyRepository.All.Select(x => new { x.Logo, x.CompanyName }).FirstOrDefault();
            //if (isCompanyLogo)
            //{
            //    if (File.Exists($"{imagePath}/{companydetail.Logo}") && !String.IsNullOrEmpty(companydetail.Logo))
            //    {
            //        FileStream logoinputstream = new FileStream($"{imagePath}/{companydetail.Logo}", FileMode.Open, FileAccess.Read);
            //        PdfImage img = new PdfBitmap(logoinputstream);
            //        var companylogo = new PointF(20, 0);
            //        header.Graphics.DrawImage(img, companylogo, imageSize);
            //    }
            //}
            //if (isClientLogo)
            //{
            //    var clientlogopath = _clientRepository.All.Where(x => x.Id == ClientId).Select(x => x.Logo).FirstOrDefault();
            //    if (File.Exists($"{imagePath}/{clientlogopath}") && !String.IsNullOrEmpty(clientlogopath))
            //    {
            //        FileStream logoinputstream = new FileStream($"{imagePath}/{clientlogopath}", FileMode.Open, FileAccess.Read);
            //        PdfImage img = new PdfBitmap(logoinputstream);
            //        var imageLocation = new PointF(doc.Pages[0].GetClientSize().Width - imageSize.Width - 20, 0);
            //        header.Graphics.DrawImage(img, imageLocation, imageSize);
            //    }
            //}

            PdfSolidBrush brush = new PdfSolidBrush(activeColor);
            PdfPen pen = new PdfPen(Color.DarkBlue, 3f);
            font = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);

            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Center;
            format.LineAlignment = PdfVerticalAlignment.Top;


            header.Graphics.DrawString($"Test Company", font, brush, new RectangleF(0, 0, header.Width, header.Height), format);
            brush = new PdfSolidBrush(Color.Gray);
            font = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);

            header.Graphics.DrawString("Volunteer Registration Form", font, brush, new RectangleF(0, 20, header.Width, header.Height), format);
            //brush = new PdfSolidBrush(Color.Gray);
            //font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Bold);
            //header.Graphics.DrawString($"Study Code :- XYZ", font, brush, new RectangleF(0, 40, header.Width, header.Height), format);


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
            string prientedby = "Printed By : " + _jwtTokenAccesser.UserName + " (" + DateTime.Now.ToString("dd-MM-yyyy h:mm tt") + ")";
            PdfCompositeField compositeFieldprintedby = new PdfCompositeField(font, brush, prientedby);
            compositeFieldprintedby.Bounds = footer.Bounds;

            compositeField.Draw(footer.Graphics, new PointF(footer.Width - 70, footer.Height - 10));

            compositeFieldprintedby.Draw(footer.Graphics, new PointF(footer.Width / 3, footer.Height - 10));

            PdfPen pen = new PdfPen(Color.Black, 1.0f);
            footer.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);

            return footer;
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
