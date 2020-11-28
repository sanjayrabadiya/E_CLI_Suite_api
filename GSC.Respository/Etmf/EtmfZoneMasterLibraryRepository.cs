using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class EtmfZoneMasterLibraryRepository : GenericRespository<EtmfZoneMasterLibrary>, IEtmfZoneMasterLibraryRepository
    {
        public EtmfZoneMasterLibraryRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(context)
        {
        }

        public List<EtmfZoneMasterLibrary> ExcelDataConvertToEntityformat(List<MasterLibraryDto> data)

        {
            List<EtmfZoneMasterLibrary> zoneLibraryList = new List<EtmfZoneMasterLibrary>();

            var objZone = data.GroupBy(u => u.Zoneno).ToList();
            foreach (var zoneObj in objZone)
            {
                EtmfZoneMasterLibrary zoneLibraryObj = new EtmfZoneMasterLibrary();

                if (!string.IsNullOrEmpty(zoneObj.Key))
                {
                    zoneLibraryObj.ZoneNo = zoneObj.Key;
                    zoneLibraryObj.EtmfSectionMasterLibrary = new List<EtmfSectionMasterLibrary>();
                    foreach (var sectionObj in zoneObj.GroupBy(x => x.SectionNo).ToList())
                    {

                        EtmfSectionMasterLibrary sectionLibraryObj = new EtmfSectionMasterLibrary();
                        sectionLibraryObj.Sectionno = sectionObj.Key;

                        sectionLibraryObj.EtmfArtificateMasterLbrary = new List<EtmfArtificateMasterLbrary>();
                        foreach (var item in sectionObj)
                        {
                            EtmfArtificateMasterLbrary artificateObj = new EtmfArtificateMasterLbrary();
                            zoneLibraryObj.ZonName = item.ZoneName;
                            zoneLibraryObj.Version = item.Version;
                            sectionLibraryObj.SectionName = item.SectionName;

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

        public string Duplicate(EtmfZoneMasterLibrary objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ZonName == objSave.ZonName && x.DeletedDate == null))
                return "Duplicate Zone name : " + objSave.ZonName;
            return "";
        }
    }
}
