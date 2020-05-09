using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignPeriodDto : BaseDto
    {
        public int ProjectDesignId { get; set; }

        [Required(ErrorMessage = "Display Name is required.")]
        public string DisplayName { get; set; }

        public string Description { get; set; }
        public List<ProjectDesignVisitDto> ProjectDesignVisits { get; set; }
        public int? AttendanceTemplateId { get; set; }
        public int? DiscontinuedTemplateId { get; set; }
    }
}