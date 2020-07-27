using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectWorkplaceSectionDto : BaseDto
    {
        public int ProjectWorkPlaceZoneId { get; set; }

        public int EtmfSectionMasterLibraryId { get; set; }

        public string SectionName { get; set; }
    }
}
