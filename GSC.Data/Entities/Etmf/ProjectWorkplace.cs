﻿using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplace : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }

        public Data.Entities.Master.Project Project { get; set; }
        public List<ProjectWorkplaceDetail> ProjectWorkplaceDetail { get; set; }
        //[NotMapped]
        //public List<Data.Entities.Master.Project> ChildProject { get; set; }
    }
}
