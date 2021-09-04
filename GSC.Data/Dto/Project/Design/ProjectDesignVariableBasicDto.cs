using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignVariableBasicDto : BaseDto
    {
        public string Value { get; set; }
        public double? InActiveVersion { get; set; }
        public double? StudyVersion { get; set; }
        public bool AllowActive { get; set; }
        public int DesignOrder { get; set; }
        public string DisplayVersion { get; set; }

    }
}
