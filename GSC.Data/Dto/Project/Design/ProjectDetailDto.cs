using System.Collections.Generic;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDetailDto
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectNumber { get; set; }
        public IEnumerable<ProjectDesignPeriodDto> ProjectDesignPeriod { get; set; }
    }
}