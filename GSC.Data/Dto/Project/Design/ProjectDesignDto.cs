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

      //  public int? CompanyId { get; set; }
        public string Version { get; set; }
        public bool IsActiveVersion { get; set; }
        public bool IsCompleteDesign { get; set; }
        public bool IsUnderTesting { get; set; }
        public string ProjectNumber { get; set; }
        public string ProjectName { get; set; }
        public string IsStatic { get; set; }
        public bool Locked { get; set; }
        public ProjectDto Project { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}