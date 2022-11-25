using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Dto.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class EtmfMasterLibrary : BaseEntity, ICommonAduit
    {
        public int EtmfMasterLibraryId { get; set; }
        public string ZoneNo { get; set; }
        public string ZonName { get; set; }
        public string Version { get; set; }
        public string FileName { get; set; }
        public string Sectionno { get; set; }
        public string SectionName { get; set; }
        public int? CompanyId { get; set; }

        public EtmfMasterLibrary EtmfZoneMasterLibrary { get; set; }
        public ICollection<EtmfMasterLibrary> EtmfSectionMasterLibrary { get; set; }
        public ICollection<EtmfArtificateMasterLbrary> EtmfArtificateMasterLbrary { get; set; }
    }
}
