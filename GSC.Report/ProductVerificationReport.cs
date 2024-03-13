using GSC.Data.Dto.Configuration;
using GSC.Respository.Client;
using GSC.Respository.Configuration;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GSC.Report
{
    public class ProductVerificationReport : IProductVerificationReport
    {
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly ICompanyRepository _companyRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IProductVerificationRepository _productVerificationRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private PdfDocument document = null;
        private PdfLayoutResult tocresult = null;
        private readonly Dictionary<PdfPageBase, int> pages = new Dictionary<PdfPageBase, int>();
        private readonly List<TocIndexCreate> _pagenumberset = new List<TocIndexCreate>();

        public ProductVerificationReport(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment,

            IJwtTokenAccesser jwtTokenAccesser,
            IUploadSettingRepository uploadSettingRepository,
            ICompanyRepository companyRepository,
            IClientRepository clientRepository,
            IProductVerificationRepository productVerificationRepository

        )
        {
            _hostingEnvironment = hostingEnvironment;
            _jwtTokenAccesser = jwtTokenAccesser;
            _companyRepository = companyRepository;
            _clientRepository = clientRepository;
            _productVerificationRepository = productVerificationRepository;
            _uploadSettingRepository = uploadSettingRepository;
           
        }

        private Stream FilePathConvert()
        {
            string path = _hostingEnvironment.WebRootPath + "/fonts/times.ttf";
            byte[] file = File.ReadAllBytes(path);
            Stream stream = new MemoryStream(file);
            return stream;
        }

        public FileStreamResult GetProductVerificationSummary(ReportSettingNew reportSetting, int ProductReceiptId)
        {
            document = new PdfDocument();
            document.PageSettings.Margins.Top = Convert.ToInt32(reportSetting.TopMargin * 100);
            document.PageSettings.Margins.Bottom = Convert.ToInt32(reportSetting.BottomMargin * 100);
            document.PageSettings.Margins.Left = Convert.ToInt32(reportSetting.LeftMargin * 100);
            document.PageSettings.Margins.Right = Convert.ToInt32(reportSetting.RightMargin * 100);
            GetProductVerification(document, ProductReceiptId, reportSetting);
            SetPageNumber();
            MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Position = 0;
            FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/pdf");
            fileStreamResult.FileDownloadName = "blankreport.pdf";
            return fileStreamResult;

        }


        private void GetProductVerification(PdfDocument document, int ProductReceiptId, ReportSettingNew reportSetting)
        {
            PdfFont headerfont = new PdfStandardFont(PdfFontFamily.TimesRoman, 14, PdfFontStyle.Bold);
            Stream fontStream = FilePathConvert();
            PdfFont regularfont = new PdfTrueTypeFont(fontStream, 12);
            var VerififcationData = _productVerificationRepository.GetProductVerificationSummary(ProductReceiptId);

            PdfSection SectionTOC = document.Sections.Add();
            PdfPage pageTOC = SectionTOC.Pages.Add();

            document.Template.Top = AddHeader(document, VerififcationData[0].StudyCode, Convert.ToBoolean(reportSetting.IsClientLogo), Convert.ToBoolean(reportSetting.IsCompanyLogo), 1);
            document.Template.Bottom = AddFooter(document);

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            layoutFormat.Layout = PdfLayoutType.Paginate;
            //not fit page then next page
            layoutFormat.Break = PdfLayoutBreakType.FitElement;


            RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            tocresult = new PdfLayoutResult(pageTOC, bounds);



            PdfStringFormat tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            PdfTextElement indexheader = new PdfTextElement("Product Verification", headerfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);
            float drectangleStarWith = 40;

            tocresult = AddString("Product Type", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].ProductType, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Product", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].ProductName, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Batch/Lot Type", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].BatchLot, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Batch/Lot No", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].BatchLotNumber, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Manufactured By", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].ManufactureBy, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Marketed By", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].MarketedBy, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Label Claim", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].LabelClaim, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Distributed By", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].DistributedBy, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Pack Desc", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].PackDesc, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Market Authorization", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].MarketAuthorization, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Mfg Date", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].MfgDate.ToString() == "" ? "" : Convert.ToDateTime(VerififcationData[0].MfgDate).ToString("dd-MM-yyyy"), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Re-Test/Expiry", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].RetestExpiry, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Re-Test/Expiry", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].RetestExpiryDate.ToString() == "" ? "" : Convert.ToDateTime(VerififcationData[0].RetestExpiryDate).ToString("dd-MM-yyyy"), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, true);

            tocresult = AddString("Product Verification Detail", tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 50, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);

            drectangleStarWith = tocresult.Bounds.Bottom;

            tocresult = AddString("Description", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].Description, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Assay Difference Verified & Meet Requirements", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            if (VerififcationData.Any() && VerififcationData[0].IsAssayRequirement.ToString() == "False")
            {
                tocresult = AddString("No", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }
            else
            {
                tocresult = AddString("YES", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);


            tocresult = AddString("Verification of IMP Done Under Sodium Vapor Lamp", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            if (VerififcationData.Any() && VerififcationData[0].IsSodiumVaporLamp.ToString() == "False")
            {
                tocresult = AddString("NO", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }
            else
            {
                tocresult = AddString("YES", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Product Description Checked With COA/SPC/PACKAGE", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            if (VerififcationData.Any() && VerififcationData[0].IsProductDescription.ToString() == "False")
            {
                tocresult = AddString("NO", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }
            else
            {
                tocresult = AddString("YES", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Condition of Products Packs", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            if (VerififcationData.Any() && VerififcationData[0].IsConditionProduct.ToString() == "False")
            {
                tocresult = AddString("Not Appropriate", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }
            else
            {
                tocresult = AddString("Appropriate", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, true);

            tocresult = AddString("No. of Boxes", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].NumberOfBox.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, pageTOC, drectangleStarWith, false);

            tocresult = AddString("No. of Qty/Boxes", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].NumberOfQty.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Received Qty", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].ReceivedQty.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Quantity of Product Use for Verification", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].QuantityVerification.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Quantity for Retention Samples Confirmed", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            if (VerififcationData.Any() && VerififcationData[0].IsRetentionConfirm.ToString() == "False")
            {
                tocresult = AddString("NO", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }
            else
            {
                tocresult = AddString("YES", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            }

            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Quantity of retention sample", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].RetentionSampleQty.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Remaining sample quantity", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].RemainingQuantity.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Storage Area", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].StorageArea, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            drectangleStarWith = DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);

            tocresult = AddString("Additional Remarks", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].Remarks, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            DrawRectangle(tocresult, tocresult.Page, drectangleStarWith, false);


            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.WordWrap = PdfWordWrapType.Word;
        }

        private float DrawRectangle(PdfLayoutResult position, PdfPage page, float startposition, bool isDraw)
        {
            //Draw Rectangle For Profile Details
            PdfSolidBrush brush = new PdfSolidBrush(new PdfColor(255, 255, 255));
            page.Graphics.Save();
            page.Graphics.SetTransparency(1, 1, PdfBlendMode.Multiply);

            if (position.Bounds.Bottom > 640)
            {
                RectangleF bound = new RectangleF(0, startposition + 5, page.GetClientSize().Width, page.GetClientSize().Height + position.Bounds.Bottom);
                page.Graphics.DrawRectangle(PdfPens.Black, brush, bound);
                return startposition;
            }
            if (isDraw)
            {
                RectangleF bound = new RectangleF(0, startposition, page.GetClientSize().Width, position.Bounds.Bottom + 5);
                page.Graphics.DrawRectangle(PdfPens.Black, brush, bound);
                return 0;
            }
            return startposition;
        }

        private static PdfLayoutResult AddString(string note, PdfPage page, RectangleF position, PdfBrush brush, PdfFont font, PdfLayoutFormat pdfLayoutFormat)
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

        private PdfPageTemplateElement AddHeader(PdfDocument doc, string studyName, bool isClientLogo, bool isCompanyLogo, int ClientId)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 70);
            PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
            Color activeColor = Color.FromArgb(44, 71, 120);
            SizeF imageSize = new SizeF(50f, 50f);

            var imagePath = _uploadSettingRepository.GetImagePath();
            var companydetail = _companyRepository.All.Select(x => new { x.Logo, x.CompanyName }).FirstOrDefault();
            if (isCompanyLogo && companydetail != null && File.Exists($"{imagePath}/{companydetail.Logo}") && !String.IsNullOrEmpty(companydetail.Logo))
            {
                FileStream logoinputstream = new FileStream($"{imagePath}/{companydetail.Logo}", FileMode.Open, FileAccess.Read);
                PdfImage img = new PdfBitmap(logoinputstream);
                var companylogo = new PointF(20, 0);
                header.Graphics.DrawImage(img, companylogo, imageSize);

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
            PdfFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);

            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Center;
            format.LineAlignment = PdfVerticalAlignment.Top;

            if (companydetail != null)
                header.Graphics.DrawString($"{companydetail.CompanyName}", font, brush, new RectangleF(0, 0, header.Width, header.Height), format);
            else
                header.Graphics.DrawString($"", font, brush, new RectangleF(0, 0, header.Width, header.Height), format);
            brush = new PdfSolidBrush(Color.Gray);
            font = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);

            header.Graphics.DrawString("Product Verification Form", font, brush, new RectangleF(0, 20, header.Width, header.Height), format);
            brush = new PdfSolidBrush(Color.Gray);
            font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Bold);
            header.Graphics.DrawString($"Study Code :- {studyName}", font, brush, new RectangleF(0, 40, header.Width, header.Height), format);


            format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.LineAlignment = PdfVerticalAlignment.Bottom;

            PdfPen pen = new PdfPen(Color.Black, 2f);
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
            string prientedby = "Printed By : " + _jwtTokenAccesser.UserName + " (" + _jwtTokenAccesser.GetClientDate().ToString("dd-MM-yyyy h:mm tt") + ")";
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
            Stream fontStream = FilePathConvert();
            PdfFont regularfont = new PdfTrueTypeFont(fontStream, 12);
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
