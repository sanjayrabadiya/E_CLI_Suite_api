using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
  public  class EtmfSectionMasterLibraryDto : BaseDto
    {
        public int EtmfZoneMasterLibraryId { get; set; }
        public string Sectionno { get; set; }
        public string SectionName { get; set; }
        public EtmfZoneMasterLibraryDto EtmfZoneMasterLibrary { get; set; }
        public ICollection<EtmfArtificateMasterLbraryDto> EtmfArtificateMasterLbrary { get; set; }
    }
}
