using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
   public class EtmfZoneMasterLibraryDto : BaseDto
    {
        public string ZoneNo { get; set; }
        public string ZonName { get; set; }
        public string Version { get; set; }

        public string FileName { get; set; }
        public ICollection<EtmfSectionMasterLibraryDto> EtmfSectionMasterLibrary { get; set; }
    }
}
