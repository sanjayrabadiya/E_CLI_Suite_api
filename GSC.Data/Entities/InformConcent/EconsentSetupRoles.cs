﻿using GSC.Data.Entities.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentSetupRoles : BaseEntity
    {
        public int EconsentDocumentId { get; set; }

        [ForeignKey("EconsentDocumentId")]
        public EconsentSetup EconsentSetup { get; set; }

        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public SecurityRole SecurityRole { get; set; }
    }
}