﻿using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplace : BaseEntity
    {
        public int ProjectId { get; set; }

        public Data.Entities.Master.Project Project { get; set; }
        public List<ProjectWorkplaceDetail> ProjectWorkplaceDetail { get; set; }
    }
}