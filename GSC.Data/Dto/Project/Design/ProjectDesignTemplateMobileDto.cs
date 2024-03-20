using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignTemplateMobileDto
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public string TemplateName { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        private DateTime? _SubmittedDate { get; set; }
        public DateTime? SubmittedDate
        {
            get => _SubmittedDate?.UtcDateTime();
            set => _SubmittedDate = value?.UtcDateTime();
        }
        public int DesignOrder { get; set; }
        private DateTime? _scheduleDate { get; set; }
        public DateTime? ScheduleDate
        {
            get => _scheduleDate?.UtcDateTime();
            set => _scheduleDate = value?.UtcDateTime();
        }

        public bool IsTemplateRestricted { get; set; }
        public bool IsPastTemplate { get; set; }
        public bool IsHide { get; set; }
        public bool IsDateTime { get; set; }
    }
}
