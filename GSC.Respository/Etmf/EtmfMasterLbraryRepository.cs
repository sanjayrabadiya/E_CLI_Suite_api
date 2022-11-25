using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
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
    }
}
