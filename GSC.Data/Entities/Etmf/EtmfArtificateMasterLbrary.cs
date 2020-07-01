using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class EtmfArtificateMasterLbrary : BaseEntity
    {
        public string ArtificateNo{ get; set; }
        public string ArtificateName { get; set; }
        public int InclutionType { get; set; }
        public bool DeviceSponDoc { get; set; }
        public bool DeviceInvesDoc { get; set; }
        public bool NondeviceSponDoc { get; set; }
        public bool NondeviceInvesDoc { get; set; }
        public string StudyArtificates { get; set; }
        public bool TrailLevelDoc { get; set; }
        public bool CountryLevelDoc { get; set; }
        public bool SiteLevelDoc { get; set; }
        public int EtmfSectionMasterLibraryId { get; set; }
        public EtmfSectionMasterLibrary EtmfSectionMasterLibrary{ get; set; }

    }
}
