using GSC.Data.Entities.Common;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class EtmfMasterLibraryDto: BaseDto
    {
        public int EtmfMasterLibraryId { get; set; }
        public string ZoneNo { get; set; }
        public string ZonName { get; set; }
        public string Version { get; set; }
        public string FileName { get; set; }
        public string Sectionno { get; set; }
        public string SectionName { get; set; }
        public int? CompanyId { get; set; }

        public EtmfMasterLibraryDto EtmfZoneMasterLibrary { get; set; }
        public ICollection<EtmfMasterLibraryDto> EtmfSectionMasterLibrary { get; set; }
        public ICollection<EtmfArtificateMasterLbraryDto> EtmfArtificateMasterLbrary { get; set; }
    }
}
