using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class TaskmasterDto: BaseDto
    {
        [Required(ErrorMessage = "Task Name is required.")]
        public string TaskName { get; set; }
        public int? ParentId { get; set; }
        [Required(ErrorMessage = "Study Tracker Template Id is required.")]
        public int TaskTemplateId { get; set; }
        public int TaskOrder { get; set; }
        public string Position { get; set; }
        [Required(ErrorMessage = "Task Type is required.")]
        public bool IsMileStone { get; set; }
        [Required(ErrorMessage = "Duration is required.")]
        public int Duration { get; set; }
        public int? DependentTaskId { get; set; }
        public ActivityType? ActivityType { get; set; }
        public int OffSet { get; set; }


    }
}
