﻿using GSC.Common.Base;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVariableRemarks : BaseEntity
    {
        public int ProjectDesignVariableId { get; set; }
        public int Range { get; set; }
        public string Remarks { get; set; }
    }
}
