using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class EtmfArtificateMasterLbrary : BaseEntity, ICommonAduit
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
        public string ArtifactCodeName { get; set; }
        public int EtmfSectionMasterLibraryId { get; set; }
        [ForeignKey("EtmfSectionMasterLibraryId")]
        public EtmfMasterLibrary EtmfSectionMasterLibrary { get; set; }

    }
}
