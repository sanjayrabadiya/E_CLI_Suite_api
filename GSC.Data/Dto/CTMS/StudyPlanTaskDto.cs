using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.CTMS
{

    public class StudyPlanTaskGridDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<StudyPlanTaskDto> StudyPlanTask { get; set; }

    }

    public class StudyPlanTaskDto : BaseAuditDto
    {
       public string TaskName { get; set; }
       public int StudyPlanId { get; set;}
       public int? ParentId { get; set; }
       public bool? IsMileStone { get; set; }
       public int Duration { get; set; }
       public DateTime? StartDate { get; set; }
       public DateTime? EndDate { get; set; }
       public int Progress { get; set; }
       public DateTime? ActualStartDate { get; set; }
       public DateTime? ActualEndDate { get; set; }
       public int TaskOrder { get; set; }
       public string Predecessor { get; set; }      
        public int? DependentTaskId { get; set; }
        public ActivityType? ActivityType { get; set; }
        public int OffSet { get; set; }
        public bool IsManual { get; set; }
        public RefrenceType RefrenceType { get; set; }
    }
   
}
