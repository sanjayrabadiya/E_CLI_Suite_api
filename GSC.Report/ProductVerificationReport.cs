using AutoMapper;
using GSC.Data.Dto.Configuration;
using GSC.Domain.Context;
using GSC.Respository.Client;
using GSC.Respository.Configuration;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private IHostingEnvironment _hostingEnvironment;
        private readonly IGSCContext _context;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IProductVerificationRepository _productVerificationRepository;
        private readonly IProductVerificationDetailRepository _productVerificationDetailRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;

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

        public ProductVerificationReport(IHostingEnvironment hostingEnvironment,
            IGSCContext context,
            IAppSettingRepository appSettingRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IUploadSettingRepository uploadSettingRepository,
            ICompanyRepository companyRepository,
            IClientRepository clientRepository,
            IMapper mapper,
            IProductVerificationRepository productVerificationRepository,
            IProductVerificationDetailRepository productVerificationDetailRepository
        )
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
            _appSettingRepository = appSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _companyRepository = companyRepository;
            _clientRepository = clientRepository;
            _mapper = mapper;
            _productVerificationRepository = productVerificationRepository;
            _productVerificationDetailRepository = productVerificationDetailRepository;
            _uploadSettingRepository = uploadSettingRepository;
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
            var VerififcationData = _productVerificationRepository.GetProductVerificationSummary(ProductReceiptId);

            PdfSection SectionTOC = document.Sections.Add();
            PdfPage pageTOC = SectionTOC.Pages.Add();

            document.Template.Top = AddHeader(document, VerififcationData[0].StudyCode, Convert.ToBoolean(reportSetting.IsClientLogo), Convert.ToBoolean(reportSetting.IsCompanyLogo), 1);
            document.Template.Bottom = AddFooter(document);

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            //layoutFormat.Break = PdfLayoutBreakType.FitPage;
            layoutFormat.Layout = PdfLayoutType.Paginate;
            //not fit page then next page
            layoutFormat.Break = PdfLayoutBreakType.FitElement;


            RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            tocresult = new PdfLayoutResult(pageTOC, bounds);



            PdfStringFormat tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            PdfTextElement indexheader = new PdfTextElement("Product Verififcation", headerfont, PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            tocresult = indexheader.Draw(tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), layoutFormat);

            PdfSolidBrush brush = new PdfSolidBrush(new PdfColor(0, 0, 0));
            PdfPen pens = new PdfPen(Color.Black);
            //Draw Rectangle For Profile Details
            brush = new PdfSolidBrush(new PdfColor(255, 255, 255));

            // Draw Line
            //pageTOC.Graphics.DrawLine(PdfPens.Black, 0, tocresult.Bounds.Bottom + 5, tocresult.Page.GetClientSize().Width, tocresult.Bounds.Bottom + 5);

            tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);


            tocresult = AddString("Product Type", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].ProductType, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Product", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].ProductName, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Batch/Lot Type", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].BatchLot, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Batch/Lot No", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].BatchLotNumber, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Manufactured By", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].ManufactureBy, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Marketed By", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].MarketedBy, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Label Claim", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].LabelClaim, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Label Claim", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].LabelClaim, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Pack Desc", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].PackDesc, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Market Authorization", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].MarketAuthorization, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Mfg Date", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].MfgDate.ToString() == "" ? "" : Convert.ToDateTime(VerififcationData[0].MfgDate).ToString("dd-MM-yyyy"), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Re-Test/Expiry", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].RetestExpiry, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Re-Test/Expiry", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].RetestExpiryDate.ToString() == "" ? "" : Convert.ToDateTime(VerififcationData[0].RetestExpiryDate).ToString("dd-MM-yyyy"), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);


            pageTOC.Graphics.Save();
            pageTOC.Graphics.SetTransparency(1, 1, PdfBlendMode.Multiply);

            bounds = new RectangleF(0, 40, pageTOC.GetClientSize().Width, tocresult.Bounds.Y);
            pageTOC.Graphics.DrawRectangle(PdfPens.Black, brush, bounds);

            tocresult = AddString("Product Verififcation Detail", tocresult.Page, new Syncfusion.Drawing.RectangleF(0, tocresult.Bounds.Bottom + 40, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, headerfont, layoutFormat);

            float drectangleStarWith = tocresult.Bounds.Bottom;

            tocresult = AddString("Quantity of Producation Use for Verification", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].QuantityVerification.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Description", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].Description, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Additional Remarks", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].Remarks, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Assay Difference Verified & Meet Requirements", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].IsAssayRequirement.ToString() == "" ? "" : VerififcationData[0].IsAssayRequirement.ToString() == "False" ? "NO" : "YES", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Quantity for Retention Samples Confirmed", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].IsRetentionConfirm.ToString() == "" ? "" : VerififcationData[0].IsRetentionConfirm.ToString() == "False" ? "NO" : "YES", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Verification of IMP Done Under Sodium Vapor Lamp", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].IsSodiumVaporLamp.ToString() == "" ? "" : VerififcationData[0].IsSodiumVaporLamp.ToString() == "False" ? "NO" : "YES", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("No. of Boxes", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].NumberOfBox.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("No. of Qty/Boxes", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].NumberOfQty.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Received Qty", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].ReceivedQty.ToString(), tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Product Description Checked With COLA/SPC/PACKAGE", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].IsProductDescription.ToString() == "" ? "" : VerififcationData[0].IsProductDescription.ToString() == "False" ? "NO" : "YES", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Storage Area", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].StorageArea, tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            tocresult = AddString("Condition of Products Packs", tocresult.Page, new Syncfusion.Drawing.RectangleF(10, tocresult.Bounds.Bottom + 10, tocresult.Page.GetClientSize().Width, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);
            tocresult = AddString(VerififcationData[0].IsConditionProduct.ToString() == "" ? "" : VerififcationData[0].IsConditionProduct.ToString() == "False" ? "NO" : "YES", tocresult.Page, new Syncfusion.Drawing.RectangleF(310, tocresult.Bounds.Y, tocresult.Page.GetClientSize().Width - 310, tocresult.Page.GetClientSize().Height), PdfBrushes.Black, regularfont, layoutFormat);

            pageTOC.Graphics.Save();
            pageTOC.Graphics.SetTransparency(1, 1, PdfBlendMode.Multiply);

            bounds = new RectangleF(0, drectangleStarWith + 5, pageTOC.GetClientSize().Width, pageTOC.GetClientSize().Height + tocresult.Bounds.Bottom);
            pageTOC.Graphics.DrawRectangle(PdfPens.Black, brush, bounds);

            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.WordWrap = PdfWordWrapType.Word;
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

            header.Graphics.DrawString("Product Verification Form", font, brush, new RectangleF(0, 20, header.Width, header.Height), format);
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
