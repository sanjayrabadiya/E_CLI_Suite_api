using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesingTemplateRestrictionDto : BaseDto
    {
        public int SecurityRoleId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string RoleName { get; set; }
        public bool IsAdd { get; set; }
        public bool IsHide { get; set; }
        public bool hasChild { get; set; }
    }
}
