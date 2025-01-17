﻿using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Syncfusion.Drawing;
using Microsoft.AspNetCore.Hosting;
using GSC.Respository.Volunteer;
using GSC.Data.Dto.Volunteer;
using System.Linq;
using GSC.Respository.Client;
using GSC.Respository.Master;

namespace GSC.Report
{
    public class VolunteerSummaryReport : IVolunteerSummaryReport
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IVolunteerAddressRepository _volunteerAddressRepository;
        public readonly IVolunteerContactRepository _volunteerContactRepository;
        public readonly IVolunteerLanguageRepository _volunteerLanguageRepository;
        public readonly IVolunteerDocumentRepository _volunteerDocumentRepository;
        public readonly IVolunteerBlockHistoryRepository _volunteerBlockHistoryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IPageConfigurationRepository _pageConfigurationRepository;
        private readonly PdfFont headerfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 14, PdfFontStyle.Bold);
        private readonly PdfFont regularfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
        private readonly PdfFont smallfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 8);
        private readonly Stream fontStream;
        private PdfDocument document = null;
        private PdfLayoutResult tocresult = null;
        PdfGrid pdfGrid = null;

        public VolunteerSummaryReport(IHostingEnvironment hostingEnvironment,
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
            IPageConfigurationRepository pageConfigurationRepository
        )
        {
            _hostingEnvironment = hostingEnvironment;
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
            _pageConfigurationRepository = pageConfigurationRepository;
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
            var volunteerPage = _pageConfigurationRepository.GetPageConfigurationByAppScreen("mnu_volunteerdetail");

            VolunteerSearchDto search = new VolunteerSearchDto();
            var volunteer = _volunteerRepository.GetVolunteerDetail(false, VolunteerID);

            document = new PdfDocument();

            document.PageSettings.Margins.Top = Convert.ToInt32(0.5 * 100);
            document.PageSettings.Margins.Bottom = Convert.ToInt32(0.5 * 100);
            document.PageSettings.Margins.Left = Convert.ToInt32(0.5 * 100);
            document.PageSettings.Margins.Right = Convert.ToInt32(0.5 * 100);

            PdfSection SectionTOC = document.Sections.Add();
            PdfPage pageTOC = SectionTOC.Pages.Add();

            document.Template.Top = AddHeader(document, true, true, 1, volunteer[0].ProfilePic);
            document.Template.Bottom = AddFooter(document);

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            layoutFormat.Layout = PdfLayoutType.Paginate;
            layoutFormat.Break = PdfLayoutBreakType.FitElement;

            RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            tocresult = new PdfLayoutResult(pageTOC, bounds);

            PdfGraphics graphics = pageTOC.Graphics;



            PdfSolidBrush brush = new PdfSolidBrush(new PdfColor(0, 0, 0));
            PdfPen pens = new PdfPen(Color.Black);

            PdfStringFormat tocformat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Top);
            PdfTextElement indexheader = new PdfTextElement();

            tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            indexheader = new PdfTextElement(volunteer[0].FullName + " (" + volunteer[0].VolunteerNo + ")", headerfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            tocformat = new PdfStringFormat(PdfTextAlignment.Right, PdfVerticalAlignment.Top);
            indexheader = new PdfTextElement(volunteer[0].AliasName, headerfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(tocresult.Bounds.X, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            //Profile Details

            if (volunteerPage.Find(q => q.ActualFieldName == "refNo").IsVisible)
            {
                tocresult = AddString("Old Reference Number", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].RefNo, tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "registerDate").IsVisible)
            {
                tocresult = AddString("Registration Date", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].RegisterDate != null ? Convert.ToDateTime(volunteer[0].RegisterDate).ToString("dd-MM-yyyy") : "", tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "dateOfBirth").IsVisible)
            {
                tocresult = AddString("Date Of Birth", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].DateOfBirth != null ? Convert.ToDateTime(volunteer[0].DateOfBirth).ToString("dd-MM-yyyy") : "", tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "fromAge").IsVisible)
            {
                tocresult = AddString("Age", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].FromAge.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "religionId").IsVisible)
            {
                tocresult = AddString("Religion", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].Religion, tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "occupationId").IsVisible)
            {
                tocresult = AddString("Occupation", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].Occupation, tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "maritalStatusId").IsVisible)
            {
                tocresult = AddString("Marital Status", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].MaritalStatus, tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "education").IsVisible)
            {
                tocresult = AddString("Education Qualification", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].Education, tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "raceId").IsVisible)
            {
                tocresult = AddString("Race", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].Race, tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "populationTypeId").IsVisible)
            {
                tocresult = AddString("Type Of Population", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].PopulationType, tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "genderId").IsVisible)
            {
                tocresult = AddString("Gender", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].Gender, tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "annualIncome").IsVisible)
            {
                tocresult = AddString("Annual Income", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].AnnualIncome.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "foodTypeId").IsVisible)
            {
                tocresult = AddString("Food Type", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].FoodType, tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "height").IsVisible)
            {
                tocresult = AddString("Height (Cm.)", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].Height.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }
            if (volunteerPage.Find(q => q.ActualFieldName == "weight").IsVisible)
            {
                tocresult = AddString("Weight (Kg.)", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].Weight.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            if (volunteerPage.Find(q => q.ActualFieldName == "bmi").IsVisible)
            {
                tocresult = AddString("BMI (Kg./Meter*Meter)", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
                tocresult = AddString(volunteer[0].BMI.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(165, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            pageTOC.Graphics.Save();
            pageTOC.Graphics.SetTransparency(1, 1, PdfBlendMode.Multiply);

            //Draw Rectangle For Profile Details
            brush = new PdfSolidBrush(new PdfColor(255, 255, 255));
            bounds = new RectangleF(0, 30, pageTOC.GetClientSize().Width, tocresult.Bounds.Y);
            pageTOC.Graphics.DrawRectangle(PdfPens.Black, brush, bounds);

            PdfGridRow header = null;

            //Creates the header style
            PdfGridCellStyle headerStyle = new PdfGridCellStyle();
            headerStyle.Borders.All = new PdfPen(new PdfColor(5, 159, 223));
            headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(5, 159, 223));
            headerStyle.TextBrush = PdfBrushes.Black;
            headerStyle.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12f, PdfFontStyle.Regular);

            //Add layout format for grid pagination
            layoutFormat = new PdfGridLayoutFormat();
            layoutFormat.Layout = PdfLayoutType.Paginate;
            layoutFormat.Break = PdfLayoutBreakType.FitElement;

            /* Address Detail Grid */
            tocresult = AddString("Address Details", tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Y + 40, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);

            DesginVoluteerAddress(VolunteerID, header, headerStyle, layoutFormat);

            if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactName").IsVisible
                || volunteerPage.Find(q => q.ActualFieldName == "defaultcontactNo").IsVisible
                || volunteerPage.Find(q => q.ActualFieldName == "defaultcontactNoTwo").IsVisible
                || volunteerPage.Find(q => q.ActualFieldName == "defaultcontactTypeId").IsVisible
                || volunteerPage.Find(q => q.ActualFieldName == "emergencycontactName").IsVisible
                || volunteerPage.Find(q => q.ActualFieldName == "emergencycontactNo").IsVisible
                || volunteerPage.Find(q => q.ActualFieldName == "emergencycontactTypeId").IsVisible
                || volunteerPage.Find(q => q.ActualFieldName == "emergencycontactTypeId").IsVisible)
            {
                /* Contact Detail Grid */
                tocresult = AddString("Contact Details", tocresult.Page, new Syncfusion.Drawing.RectangleF(0, (tocresult.Bounds.Bottom + 10) > 605 ? (tocresult.Page.GetClientSize().Height + 10) : (tocresult.Bounds.Bottom + 10), tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);

                DesginVoluteerContact(VolunteerID, header, headerStyle, layoutFormat);
            }

            /* Language Detail Grid */
            tocresult = AddString("Language Details", tocresult.Page, new Syncfusion.Drawing.RectangleF(0, (tocresult.Bounds.Bottom + 10) > 605 ? (tocresult.Page.GetClientSize().Height + 10) : (tocresult.Bounds.Bottom + 10), tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);

            DesginVoluteerLanguage(VolunteerID, header, headerStyle, layoutFormat);

            /* Documnet Detail Grid */
            tocresult = AddString("Document Details", tocresult.Page, new Syncfusion.Drawing.RectangleF(0, (tocresult.Bounds.Bottom + 10) > 605 ? (tocresult.Page.GetClientSize().Height + 10) : (tocresult.Bounds.Bottom + 10), tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);

            DesginVoluteerDocument(VolunteerID, header, headerStyle, layoutFormat);

            /* Documnet Grid */
            DesignVoluteerDocumentShow(VolunteerID, header, headerStyle, layoutFormat, graphics, document);

            DesignVoluteerDocumentShowPdf(VolunteerID, document);

            //Save and the document
            MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Position = 0;
            FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/pdf");
            fileStreamResult.FileDownloadName = "blankreport.pdf";
            return fileStreamResult;
        }

        private void DesginVoluteerAddress(int VolunteerID, PdfGridRow header, PdfGridCellStyle headerStyle, PdfLayoutFormat pdfLayoutFormat)
        {
            var volunteerAddress = _volunteerAddressRepository.GetAddresses(VolunteerID);
            var volunteerPage = _pageConfigurationRepository.GetPageConfigurationByAppScreen("mnu_volunteerdetail");
            //Creates the datasource for the table
            DataTable addressDetails = new DataTable();

            if (volunteerPage.Find(q => q.ActualFieldName == "address").IsVisible)
                addressDetails.Columns.Add("Address");

            if (volunteerPage.Find(q => q.ActualFieldName == "countryId").IsVisible)
                addressDetails.Columns.Add("Country");

            if (volunteerPage.Find(q => q.ActualFieldName == "stateId").IsVisible)
                addressDetails.Columns.Add("State");

            if (volunteerPage.Find(q => q.ActualFieldName == "cityId").IsVisible)
                addressDetails.Columns.Add("City");

            if (volunteerPage.Find(q => q.ActualFieldName == "cityAreaId").IsVisible)
                addressDetails.Columns.Add("City Area");

            if (volunteerPage.Find(q => q.ActualFieldName == "zip").IsVisible)
                addressDetails.Columns.Add("Zip/Post Code");

            addressDetails.Columns.Add("Current Address?");
            addressDetails.Columns.Add("Permanent Address?");


            if (volunteerAddress.Count > 0)
            {
                foreach (var address in volunteerAddress)
                {

                    var dataRow = addressDetails.NewRow();

                    if (volunteerPage.Find(q => q.ActualFieldName == "address").IsVisible)
                        dataRow["Address"] = address.Location.Address;

                    if (volunteerPage.Find(q => q.ActualFieldName == "countryId").IsVisible)
                        dataRow["Country"] = address.Location.CountryName;

                    if (volunteerPage.Find(q => q.ActualFieldName == "stateId").IsVisible)
                        dataRow["State"] = address.Location.StateName;

                    if (volunteerPage.Find(q => q.ActualFieldName == "cityId").IsVisible)
                        dataRow["City"] = address.Location.CityName;

                    if (volunteerPage.Find(q => q.ActualFieldName == "cityAreaId").IsVisible)
                        dataRow["City Area"] = address.Location.CityAreaName;

                    if (volunteerPage.Find(q => q.ActualFieldName == "zip").IsVisible)
                        dataRow["Zip/Post Code"] = address.Location.Zip;

                    dataRow["Current Address?"] = address.IsCurrent ? "YES" : "NO";
                    dataRow["Permanent Address?"] = address.IsPermanent ? "YES" : "NO";
                    addressDetails.Rows.Add(dataRow);
                }
            }
            else
            {
                addressDetails.Rows.Add(new object[] {
                                                      "No Record Found"
                                                });
            }


            /* Address Detail Grid */
            pdfGrid = new PdfGrid();
            //Assign data source
            pdfGrid.DataSource = addressDetails;
            pdfGrid.AllowRowBreakAcrossPages = true;
            if (pdfGrid.Rows.Count == 1)
            {
                PdfGridCell Addresscell = pdfGrid.Rows[0].Cells[0];
                if (Addresscell.Value.ToString() == "No Record Found")
                {
                    Addresscell.ColumnSpan = pdfGrid.Columns.Count;
                    Addresscell.Value = "No Record Found.";
                    Addresscell.StringFormat.Alignment = PdfTextAlignment.Justify;
                }
            }

            //Draw grid to the page of PDF document
            header = pdfGrid.Headers[0];
            //Adds cell customizations
            for (int i = 0; i < pdfGrid.Columns.Count; i++)
            {
                pdfGrid.Columns[i].Format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);

            }
            header.ApplyStyle(headerStyle);
            tocresult = pdfGrid.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), pdfLayoutFormat);
        }

        private void DesginVoluteerContact(int VolunteerID, PdfGridRow header, PdfGridCellStyle headerStyle, PdfLayoutFormat pdfLayoutFormat)
        {
            var volunteercontact = _volunteerContactRepository.GetContactTypeList(VolunteerID);

            var volunteerPage = _pageConfigurationRepository.GetPageConfigurationByAppScreen("mnu_volunteerdetail");

            //Creates the datasource for the table
            DataTable contactDetails = new DataTable();

            if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactName").IsVisible)
                contactDetails.Columns.Add("Contact Name");
            if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactNo").IsVisible)
                contactDetails.Columns.Add("Contact No.");
            if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactNoTwo").IsVisible)
                contactDetails.Columns.Add("Contact No. 2");
            if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactTypeId").IsVisible)
                contactDetails.Columns.Add("Contact");



            if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactName").IsVisible
                || volunteerPage.Find(q => q.ActualFieldName == "defaultcontactNo").IsVisible
                || volunteerPage.Find(q => q.ActualFieldName == "defaultcontactNoTwo").IsVisible
                || volunteerPage.Find(q => q.ActualFieldName == "defaultcontactTypeId").IsVisible)
                contactDetails.Columns.Add("Default Contact?");

            if (volunteerPage.Find(q => q.ActualFieldName == "emergencycontactNo").IsVisible
               || volunteerPage.Find(q => q.ActualFieldName == "emergencycontactTypeId").IsVisible
               || volunteerPage.Find(q => q.ActualFieldName == "emergencycontactName").IsVisible)
                contactDetails.Columns.Add("Emergency Contact?");


            if (volunteercontact.Count > 0)
            {
                foreach (var contact in volunteercontact)
                {

                    var dataRow = contactDetails.NewRow();

                    if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactTypeId").IsVisible)
                        dataRow["Contact"] = contact.ContactTypeName;

                    if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactNo").IsVisible)
                        dataRow["Contact No."] = contact.ContactNo;

                    if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactNoTwo").IsVisible)
                        dataRow["Contact No. 2"] = contact.ContactNoTwo;

                    if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactName").IsVisible)
                        dataRow["Contact Name"] = contact.ContactName;


                    if (volunteerPage.Find(q => q.ActualFieldName == "defaultcontactName").IsVisible
                        || volunteerPage.Find(q => q.ActualFieldName == "defaultcontactNo").IsVisible
                        || volunteerPage.Find(q => q.ActualFieldName == "defaultcontactNoTwo").IsVisible
                        || volunteerPage.Find(q => q.ActualFieldName == "defaultcontactTypeId").IsVisible)
                        dataRow["Default Contact?"] = contact.IsDefault ? "YES" : "NO";

                    if (volunteerPage.Find(q => q.ActualFieldName == "emergencycontactNo").IsVisible
                       || volunteerPage.Find(q => q.ActualFieldName == "emergencycontactTypeId").IsVisible
                       || volunteerPage.Find(q => q.ActualFieldName == "emergencycontactName").IsVisible)
                        dataRow["Emergency Contact?"] = contact.IsEmergency ? "YES" : "NO";

                    contactDetails.Rows.Add(dataRow);
                }
            }
            else
            {
                contactDetails.Rows.Add(new object[] {
                                                      "No Record Found"
                                                });
            }

            //Create a PdfGrid
            pdfGrid = new PdfGrid();
            pdfGrid.DataSource = contactDetails;
            pdfGrid.AllowRowBreakAcrossPages = true;
            if (pdfGrid.Rows.Count == 1)
            {
                PdfGridCell Contactcell = pdfGrid.Rows[0].Cells[0];
                if (Contactcell.Value.ToString() == "No Record Found")
                {
                    Contactcell.ColumnSpan = pdfGrid.Columns.Count;
                    Contactcell.Value = "No Record Found.";
                    Contactcell.StringFormat.Alignment = PdfTextAlignment.Justify;
                }
            }
            header = pdfGrid.Headers[0];
            header.ApplyStyle(headerStyle);
            for (int i = 0; i < pdfGrid.Columns.Count; i++)
            {
                pdfGrid.Columns[i].Format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);

            }
            tocresult = pdfGrid.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), pdfLayoutFormat);
        }

        private void DesginVoluteerLanguage(int VolunteerID, PdfGridRow header, PdfGridCellStyle headerStyle, PdfLayoutFormat pdfLayoutFormat)
        {
            var volunteerLanguage = _volunteerLanguageRepository.GetLanguages(VolunteerID, false);

            //Creates the datasource for the table
            DataTable languageDetails = new DataTable();

            languageDetails.Columns.Add("Language Name");
            //languageDetails.Columns.Add("Note");   //Hide this column by Tinku (told by Urvi 03/08/2022)
            languageDetails.Columns.Add("Read & Understood?");
            languageDetails.Columns.Add("Write?");
            languageDetails.Columns.Add("Speak?");

            if (volunteerLanguage.Count > 0)
            {
                foreach (var language in volunteerLanguage)
                {

                    languageDetails.Rows.Add(new object[] {
                                                      language.LanguageName
                                                    , language.IsRead ? "YES" : "NO"
                                                    , language.IsWrite ? "YES":"NO"
                                                    , language.IsSpeak ? "YES":"NO"
                                                });
                }
            }
            else
            {
                languageDetails.Rows.Add(new object[] {
                                                      "No Record Found"
                                                });
            }

            //Create a PdfGrid
            pdfGrid = new PdfGrid();
            //Assign data source
            pdfGrid.DataSource = languageDetails;
            pdfGrid.AllowRowBreakAcrossPages = true;
            if (pdfGrid.Rows.Count == 1)
            {
                PdfGridCell Languagecell = pdfGrid.Rows[0].Cells[0];
                if (Languagecell.Value.ToString() == "No Record Found")
                {
                    Languagecell.ColumnSpan = pdfGrid.Columns.Count;
                    Languagecell.Value = "No Record Found.";
                    Languagecell.StringFormat.Alignment = PdfTextAlignment.Justify;
                }
            }
            header = pdfGrid.Headers[0];
            //Adds cell customizations
            header.ApplyStyle(headerStyle);
            for (int i = 0; i < pdfGrid.Columns.Count; i++)
            {
                pdfGrid.Columns[i].Format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);

            }
            //Draw grid to the resultant page of the first grid
            tocresult = pdfGrid.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), pdfLayoutFormat);
        }

        private void DesginVoluteerDocument(int VolunteerID, PdfGridRow header, PdfGridCellStyle headerStyle, PdfLayoutFormat pdfLayoutFormat)
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var volunteerDocument = _volunteerDocumentRepository
                .FindByInclude(t => t.VolunteerId == VolunteerID && t.DeletedDate == null, t => t.DocumentType,
                    t => t.DocumentName).OrderByDescending(x => x.Id).ToList();
            volunteerDocument.ForEach(t => t.PathName = documentUrl + t.PathName);


            //Creates the datasource for the table
            DataTable documentDetails = new DataTable();

            documentDetails.Columns.Add("Document Type");
            documentDetails.Columns.Add("Document Name");
            documentDetails.Columns.Add("Remark");


            if (volunteerDocument.Count > 0)
            {
                foreach (var document in volunteerDocument)
                {

                    documentDetails.Rows.Add(new object[] {
                                                      document.DocumentType.TypeName
                                                    , document.DocumentName.Name
                                                    , document.Note
                                                });
                }
            }
            else
            {
                documentDetails.Rows.Add(new object[] {
                                                      "No Record Found"
                                                });
            }

            pdfGrid = new PdfGrid();
            pdfGrid.DataSource = documentDetails;
            pdfGrid.AllowRowBreakAcrossPages = true;
            if (pdfGrid.Rows.Count == 1)
            {
                PdfGridCell VolDocumnetcell = pdfGrid.Rows[0].Cells[0];
                if (VolDocumnetcell.Value.ToString() == "No Record Found")
                {
                    VolDocumnetcell.ColumnSpan = 3;
                    VolDocumnetcell.Value = "No Record Found.";
                    VolDocumnetcell.StringFormat.Alignment = PdfTextAlignment.Justify;
                }
            }
            header = pdfGrid.Headers[0];
            header.ApplyStyle(headerStyle);
            for (int i = 0; i < pdfGrid.Columns.Count; i++)
            {
                pdfGrid.Columns[i].Format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);

            }
            //Draw grid to the resultant page of the first grid
            tocresult = pdfGrid.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), pdfLayoutFormat);
        }

        private static Stream GetStreamFromUrl(string url)
        {
            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
                imageData = wc.DownloadData(url);

            return new MemoryStream(imageData);
        }

        private void DesignVoluteerDocumentShow(int VolunteerID, PdfGridRow header, PdfGridCellStyle headerStyle, PdfLayoutFormat pdfLayoutFormat, PdfGraphics graphics, PdfDocument document)
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var volunteerDocument = _volunteerDocumentRepository
                .FindByInclude(t => t.VolunteerId == VolunteerID && t.DeletedDate == null, t => t.DocumentType,
                    t => t.DocumentName).OrderByDescending(x => x.Id).ToList();
            volunteerDocument.ForEach(t => t.PathName = documentUrl + t.PathName);

            if (volunteerDocument.Count > 0)
            {
                foreach (var data in volunteerDocument.Where(x => x.MimeType == "jpeg" || x.MimeType == "jpg" || x.MimeType == "png"))
                {
                    PdfPage page = document.Pages.Add();
                    int endIndex = document.Pages.Count - 1;
                    PdfBitmap image = new PdfBitmap(GetStreamFromUrl(data.PathName));

                    PdfLayoutFormat format = new PdfLayoutFormat();
                    format.Break = PdfLayoutBreakType.FitPage;
                    format.Layout = PdfLayoutType.OnePage;
                    RectangleF imageBounds = new RectangleF(0, 0, 500, 600);

                    page.Graphics.DrawImage(image, 0, 10, 500, 500);
                }
            }
        }

        private void DesignVoluteerDocumentShowPdf(int VolunteerID, PdfDocument document)
        {
            var documentPath = _uploadSettingRepository.GetDocumentPath();
            var volunteerDocument = _volunteerDocumentRepository
                .FindByInclude(t => t.VolunteerId == VolunteerID && t.MimeType == "pdf" && t.DeletedDate == null, t => t.DocumentType,
                    t => t.DocumentName).OrderByDescending(x => x.Id).ToList();

            PdfMergeOptions mergeOptions = new PdfMergeOptions();
            List<Stream> pdfStreams = new List<Stream>();
            foreach (var item in volunteerDocument)
            {
                var PathName = documentPath + item.PathName;
                Stream stream2 = File.OpenRead(PathName);
                pdfStreams.Add(stream2);
            }
            mergeOptions.OptimizeResources = true;
            mergeOptions.ExtendMargin = true;
            PdfDocumentBase.Merge(document, mergeOptions, pdfStreams.Cast<object>().ToArray());
        }

        private PdfPageTemplateElement AddHeader(PdfDocument doc, bool isClientLogo, bool isCompanyLogo, int ClientId, string ProfilePic)
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
            if (ProfilePic != "")
            {
                imageSize = new SizeF(65f, 65f);
                if (File.Exists($"{imagePath}/{ProfilePic}"))
                {
                    FileStream VoluntreerProfileinputstream = new FileStream($"{imagePath}/{ProfilePic}", FileMode.Open, FileAccess.Read);
                    PdfImage img = new PdfBitmap(VoluntreerProfileinputstream);
                    var VoluntreerProfile = new PointF(doc.Pages[0].GetClientSize().Width - 75, 0);
                    header.Graphics.DrawImage(img, VoluntreerProfile, imageSize);
                    header.Graphics.DrawRectangle(new PdfPen(Color.Black), new RectangleF(doc.Pages[0].GetClientSize().Width - 75, 0, 65f, 65f));

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

            header.Graphics.DrawString("Volunteer Registration Form", font, brush, new RectangleF(0, 20, header.Width, header.Height), format);

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
            string prientedby = "";
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
            PdfTextElement richTextElement = new PdfTextElement(String.IsNullOrEmpty(note) ? " " : note, font, brush);
            //Draws String       
            PdfStringFormat stringFormat = new PdfStringFormat();
            stringFormat.MeasureTrailingSpaces = true;
            stringFormat.WordWrap = PdfWordWrapType.Word;
            richTextElement.StringFormat = stringFormat;

            PdfLayoutResult result = richTextElement.Draw(page, position, pdfLayoutFormat);
            return result;
        }

        public FileStreamResult GetVolunteerSearchDesign(IList<VolunteerGridDto> volunteerGridDto)
        {
            var MainData = volunteerGridDto.ToList();
            using (PdfDocument doc = new PdfDocument())
            {
                //Set the orientation
                doc.PageSettings.Orientation = PdfPageOrientation.Landscape;

                //Add a page
                PdfPage page = doc.Pages.Add();

                //Add header 
                doc.Template.Top = AddSearchDesignHeader(doc, "Volunteer Details");

                //Add footer 
                doc.Template.Bottom = AddSearchDesignFooter(doc);

                //Create a PdfGrid
                PdfGrid pdfGrid = new PdfGrid();

                //Create a DataTable
                DataTable dataTable = new DataTable();

                //Add columns to the DataTable
                dataTable.Columns.Add("CODE");
                dataTable.Columns.Add("REF");
                dataTable.Columns.Add("DATE OF BIRTH");
                dataTable.Columns.Add("NAME");
                dataTable.Columns.Add("INITIAL");
                dataTable.Columns.Add("REGISTER DATE");
                dataTable.Columns.Add("RELIGION");
                dataTable.Columns.Add("OCCUPATION");
                dataTable.Columns.Add("EDUCATION");
                dataTable.Columns.Add("RACE");
                dataTable.Columns.Add("MARITAL STATUS");
                dataTable.Columns.Add("POPULATION");
                dataTable.Columns.Add("GENDER");
                dataTable.Columns.Add("ANNUAL INCOME");
                dataTable.Columns.Add("STATUS");
                dataTable.Columns.Add("BLOCKED");
                dataTable.Columns.Add("CREATED BY");
                dataTable.Columns.Add("CREATED DATE");

                //Add rows to the DataTable
                MainData.ForEach(d =>
                {
                    dataTable.Rows.Add(new object[] { d.VolunteerNo, d.RefNo, Convert.ToDateTime(d.DateOfBirth).ToString("dd/MMM/yyyy"), d.FullName, d.AliasName,
                        Convert.ToDateTime(d.RegisterDate).ToString("dd/MMM/yyyy"),d.Religion, d.Occupation, d.Education, d.Race,
                    d.MaritalStatus,d.PopulationType,d.Gender,d.AnnualIncome,d.StatusName,d.Blocked,d.CreatedByUser,
                        Convert.ToDateTime(d.CreatedDate).ToString("dd/MMM/yyyy HH:mm")});
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

                //Save and the document
                MemoryStream memoryStream = new MemoryStream();
                doc.Save(memoryStream);
                memoryStream.Position = 0;
                FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/pdf");
                fileStreamResult.FileDownloadName = "blankreport.pdf";
                return fileStreamResult;
            }
        }

        public PdfPageTemplateElement AddSearchDesignHeader(PdfDocument doc, string title)
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

        public PdfPageTemplateElement AddSearchDesignFooter(PdfDocument doc)
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
