﻿using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class VisitEmailConfigurationDto : BaseDto
    {
        public int ProjectDesignVisitId { get; set; }
        public string EmailBody { get; set; }
        public string Subject { get; set; }
        public int[] RoleId { get; set; }
        public ScreeningVisitStatus VisitStatusId { get; set; }
        public List<VisitEmailConfigurationRolesDto> VisitEmailConfigurationRoles { get; set; }
    }

    public class VisitEmailConfigurationGridDto : BaseAuditDto
    {
        public string VisitName { get; set; }
        public string Subject { get; set; }
        public string VisitStatus { get; set; }
        public string UserRoles { get; set; }
        public string EmailBody { get; set; }
    }
}
