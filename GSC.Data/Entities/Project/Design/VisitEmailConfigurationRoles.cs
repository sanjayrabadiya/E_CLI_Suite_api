﻿using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class VisitEmailConfigurationRoles : BaseEntity, ICommonAduit
    {
        public int VisitEmailConfigurationId { get; set; }
        public int SecurityRoleId { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public VisitEmailConfiguration VisitEmailConfiguration { get; set; }
    }
}
