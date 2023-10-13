using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Design
{
    public class VisitEmailConfigurationRolesDto: BaseDto
    {
        public int VisitEmailConfigurationId { get; set; }
        public int SecurityRoleId { get; }
    }
}
