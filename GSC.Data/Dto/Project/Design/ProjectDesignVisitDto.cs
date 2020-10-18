using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignVisitDto : BaseDto
    {
        public int ProjectDesignPeriodId { get; set; }

        [Required(ErrorMessage = "Display Name is required.")]
        public string DisplayName { get; set; }

        public string Description { get; set; }
        public bool IsRepeated { get; set; }
        public bool? IsSchedule { get; set; }
        public List<ProjectDesignTemplateDto> Templates { get; set; }
    }


    public class ProjectDesignVisitBasicDto
    {
        public int Id { get; set; }
        public bool IsRepeated { get; set; }
        public bool? IsSchedule { get; set; }
        public List<int> Templates { get; set; }
    }
}