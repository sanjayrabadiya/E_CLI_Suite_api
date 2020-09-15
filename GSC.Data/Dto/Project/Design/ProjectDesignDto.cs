using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignDto : BaseDto
    {
        [Required(ErrorMessage = "Project Name is required.")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Period is required.")]
        public int Period { get; set; }
        public string Version { get; set; }
        public bool IsActiveVersion { get; set; }
        public bool IsCompleteDesign { get; set; }
        public bool IsUnderTesting { get; set; }
        public string ProjectNumber { get; set; }
        public string ProjectName { get; set; }
        public bool IsStatic { get; set; }
        public bool Locked { get; set; }
        public ProjectDto Project { get; set; }
        public int? CompanyId { get; set; }
    }
}