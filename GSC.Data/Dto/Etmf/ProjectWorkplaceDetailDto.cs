using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectWorkplaceDetailDto : BaseDto
    {
        public int ProjectWorkplaceId { get; set; }

        public int WorkPlaceFolderId { get; set; }
        public int ItemId{ get; set; }
        public string ItemName { get; set; }

        public List<ProjectWorkplaceDetailDto> ProjectWorkplaceCountry { get; set; }
        public List<ProjectWorkplaceDetailDto> ProjectWorkplaceSite{ get; set; }
        public List<ProjectWorkplaceDetailDto> ProjectWorkplaceTrial { get; set; }

    }
}
