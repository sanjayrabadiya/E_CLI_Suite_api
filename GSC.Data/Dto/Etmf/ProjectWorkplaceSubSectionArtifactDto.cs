using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectWorkplaceSubSectionArtifactDto : BaseDto
    {
        public int ProjectWorkplaceSubSectionId { get; set; }
        public string ArtifactName { get; set; }

        public string SubSectionName { get; set; }
        public string SectionName { get; set; }
        public int ProjectWorkplaceZoneId { get; set; }
        public string ZonName { get; set; }
        public int WorkPlaceFolderId { get; set; }
        public string ChildName { get; set; }
        public string ProjectName { get; set; }
        public int projectWorkplaceDetailId { get; set; }
        public bool IsNotRequired { get; set; }
    }
}
