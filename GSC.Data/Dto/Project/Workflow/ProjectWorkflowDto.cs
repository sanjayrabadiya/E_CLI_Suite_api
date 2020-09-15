using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Workflow
{
    public class ProjectWorkflowDto : BaseDto
    {
        [Required(ErrorMessage = "Project Name is required.")]
        public int ProjectDesignId { get; set; }
        public bool IsIndependent { get; set; }
        public string ProjectName { get; set; }
        public bool IsLock { get; set; }
        public IList<ProjectWorkflowLevelDto> Levels { get; set; }
        public IList<ProjectWorkflowIndependentDto> Independents { get; set; }
        
        public int? CompanyId { get; set; }
        public bool? IsCompleteWorkflow { get; set; }
    }

    public class WorkFlowLevelDto
    {
        public bool SelfCorrection { get; set; }
        public bool IsStartTemplate { get; set; }
        public bool IsWorkFlowBreak { get; set; }
        public bool IsGenerateQuery { get; set; }
        public bool IsLock { get; set; }
        public short LevelNo { get; set; }
        public int TotalLevel { get; set; }
        public short StartLevel { get; set; }
        public bool IsElectricSignature { get; set; }
        public List<WorkFlowText> WorkFlowText { get; set; }
    }

    public class WorkFlowText
    {
        public short LevelNo { get; set; }
        public string RoleName { get; set; }
    }
}