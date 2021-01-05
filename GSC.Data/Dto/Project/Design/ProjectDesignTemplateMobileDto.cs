﻿using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignTemplateMobileDto
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string TemplateName { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public int DesignOrder { get; set; }
    }
}
