using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignVisitMobileDto : BaseDto
    {
        public string DisplayName { get; set; }
        public int ScreeningEntryId { get; set; }
    }
}
