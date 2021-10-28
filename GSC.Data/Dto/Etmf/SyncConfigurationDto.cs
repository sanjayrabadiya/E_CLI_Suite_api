using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class SyncConfigurationDto: BaseDto
    {
        [Required(ErrorMessage = "Project Name is required.")]
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Module is required.")]
        public int AppScreenId { get; set; }
        public int ProjectWorkplaceSubSectionArtifactId { get; set; }
        public int ProjectWorkplaceArtificateId { get; set; }

        public int ProjectWorkplaceDetailId { get; set; }
        public int ProjectWorkPlaceZoneId { get; set; }
        public int ProjectWorkplaceSectionId { get; set; }
        public int ArtifactType { get; set;}
        public int ProjectWorkplaceSubSectionId { get; set; }    
    }
}
