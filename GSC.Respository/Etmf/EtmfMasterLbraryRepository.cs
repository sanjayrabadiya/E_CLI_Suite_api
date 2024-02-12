using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class EtmfMasterLbraryRepository : GenericRespository<EtmfMasterLibrary>, IEtmfMasterLbraryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public EtmfMasterLbraryRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        public string Duplicate(EtmfMasterLibrary objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.Version == objSave.Version.Trim() && x.DeletedDate == null))
                return "Duplicate Zone name : " + objSave.ZonName;
            return "";
        }

        public List<EtmfMasterLibrary> ExcelDataConvertToEntityformat(List<MasterLibraryDto> data)
        {
            List<EtmfMasterLibrary> zoneLibraryList = new List<EtmfMasterLibrary>();

            var objZone = data.GroupBy(u => u.Zoneno).ToList();
            foreach (var zoneObj in objZone)
            {
                EtmfMasterLibrary zoneLibraryObj = new EtmfMasterLibrary();

                if (!string.IsNullOrEmpty(zoneObj.Key))
                {
                    zoneLibraryObj.Version = zoneObj.Key;
                    zoneLibraryObj.EtmfMasterLibraryId = 0;
                    zoneLibraryObj.EtmfSectionMasterLibrary = new List<EtmfMasterLibrary>();
                    foreach (var sectionObj in zoneObj.GroupBy(x => x.SectionNo).ToList())
                    {

                        EtmfMasterLibrary sectionLibraryObj = new EtmfMasterLibrary();
                        sectionLibraryObj.Version = sectionObj.Key;

                        sectionLibraryObj.EtmfArtificateMasterLbrary = new List<EtmfArtificateMasterLbrary>();
                        foreach (var item in sectionObj)
                        {
                            EtmfArtificateMasterLbrary artificateObj = new EtmfArtificateMasterLbrary();
                            zoneLibraryObj.ZonName = item.ZoneName;
                            zoneLibraryObj.Version = item.Version;
                            sectionLibraryObj.SectionName = item.SectionName;
                            sectionLibraryObj.Sectionno = item.SectionNo;
                            sectionLibraryObj.EtmfMasterLibraryId = zoneLibraryObj.EtmfMasterLibraryId;

                            artificateObj.ArtificateName = item.ArtificateName;
                            artificateObj.ArtificateNo = item.ArtificateNo;
                            artificateObj.InclutionType = item.InclusionType;
                            artificateObj.DeviceSponDoc = item.DeviceSponDoc == "X" ? true : false;
                            artificateObj.DeviceInvesDoc = item.DeviceInvesDoc == "X" ? true : false;

                            artificateObj.NondeviceSponDoc = item.NondeviceSponDoc == "X" ? true : false;
                            artificateObj.NondeviceInvesDoc = item.NondeviceInvesDoc == "X" ? true : false;

                            artificateObj.StudyArtificates = item.StudyArtificates;
                            artificateObj.TrailLevelDoc = item.TrailLevelDoc == "X" ? true : false;
                            artificateObj.CountryLevelDoc = item.CountryLevelDoc == "X" ? true : false;
                            artificateObj.SiteLevelDoc = item.SiteLevelDoc == "X" ? true : false;
                            artificateObj.ArtifactCodeName = item.ArtifactCodeName;

                            sectionLibraryObj.EtmfArtificateMasterLbrary.Add(artificateObj);
                        }

                        zoneLibraryObj.EtmfSectionMasterLibrary.Add(sectionLibraryObj);
                    }
                    zoneLibraryList.Add(zoneLibraryObj);

                }
            }
            return zoneLibraryList;
        }

        public List<DropDownDto> GetSectionMasterLibraryDropDown(int EtmfZoneMasterLibraryId)
        {
            return All.Where(x => x.EtmfMasterLibraryId == EtmfZoneMasterLibraryId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.SectionName }).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetZoneMasterLibraryDropDown(string version)
        {
            return All.Where(x => x.Version == version && x.EtmfMasterLibraryId == 0)
                    .Select(c => new DropDownDto { Id = c.Id, Value = c.ZonName }).OrderBy(o => o.Value).ToList();
        }


        public PdfPageTemplateElement AddFooter(PdfDocument doc, string documentName, string version)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 10);
            PdfPageTemplateElement footer = new PdfPageTemplateElement(rect);
            PdfFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 8, PdfFontStyle.Bold);
            PdfSolidBrush brush = new PdfSolidBrush(Color.Black);

            PdfPageNumberField pageNumber = new PdfPageNumberField(font, brush);
            PdfPageCountField count = new PdfPageCountField(font, brush);

            PdfCompositeField compositeField = new PdfCompositeField(font, brush, "Page {0} of {1}", pageNumber, count);
            compositeField.Bounds = footer.Bounds;

            string strDocument = $"Document Name : {documentName} Version : {version} Printed By : {_jwtTokenAccesser.UserName} ({_jwtTokenAccesser.GetClientDate().ToString("dd-MMM-yyyy h:mm tt")})";
            PdfCompositeField compositeFieldDocumentNmae = new PdfCompositeField(font, brush, strDocument);
            compositeFieldDocumentNmae.Bounds = footer.Bounds;

            compositeField.Draw(footer.Graphics, new PointF(footer.Width - 70, footer.Height - 10));
            compositeFieldDocumentNmae.Draw(footer.Graphics, new PointF(10, footer.Height - 10));

            PdfPen pen = new PdfPen(Color.Black, 1.0f);
            footer.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);

            return footer;
        }
    }
}
