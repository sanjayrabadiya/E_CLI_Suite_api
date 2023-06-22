using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignVisitRestrictionDto : BaseDto
    {
        public int SecurityRoleId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public string RoleName { get; set; }
        public bool IsAdd { get; set; }
        public bool hasChild { get; set; }
    }
}
