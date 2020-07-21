using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectWorkplaceArtificateDto : BaseDto
    {
        public int ProjectWorkplaceSectionId { get; set; }
        public int EtmfArtificateMasterLbraryId { get; set; }
    }
}
