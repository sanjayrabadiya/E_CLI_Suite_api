using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectWorkplaceSubSectionDto : BaseDto
    {
        public int ProjectWorkplaceSectionId { get; set; }
        public string SubSectionName { get; set; }
        public string  SectionName { get; set; }
        public int ProjectWorkplaceZoneId { get; set; }
        public string ZonName { get; set; }
        public int WorkPlaceFolderId { get; set; }
        public string ChildName { get; set; }
        public string ProjectName { get; set; }
        public int projectWorkplaceDetailId { get; set; }
        public string SubSectionArtifactName { get; set; }
        public int CompanyId { get; set; }
    }
}
