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
        public bool IsNonCRF { get; set; }
        public bool InActive { get; set; }
        public bool AllowActive { get; set; }
        public int? DesignOrder { get; set; }
        public double? InActiveVersion { get; set; }
        public string DisplayVersion { get; set; }
        public double? StudyVersion { get; set; }
        public string PreLabel { get; set; }
        public List<ProjectDesignTemplateDto> Templates { get; set; }
    }


    public class ProjectDesignVisitBasicDto
    {
        public int Id { get; set; }
        public bool IsRepeated { get; set; }
        public bool? IsSchedule { get; set; }
        public double? StudyVersion { get; set; }
        public double? InActiveVersion { get; set; }
        public bool IsNonCRF { get; set; }
        public List<InsertScreeningTemplate> Templates { get; set; }
    }

    public class InsertScreeningTemplate
    {
        public int ProjectDesignTemplateId { get; set; }
        public double? StudyVersion { get; set; }
        public double? InActiveVersion { get; set; }
    }

    public class ProjectDesignVisitClone
    {
        public int Id { get; set; }
        public int projectDesignPeriodId { get; set; }
        public int noOfVisits { get; set; }
        public int[] noOfTemplate { get; set; }
    }
}