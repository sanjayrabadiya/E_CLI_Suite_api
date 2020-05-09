using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.ProjectRight
{
    public class ProjectRightListDto : BaseDto
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string Users { get; set; }
        public string Documents { get; set; }
    }
}