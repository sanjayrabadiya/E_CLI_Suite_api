using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.Master
{
    public class TemplateRightsRoleListDto : BaseDto
    {
        public string SecurityRoleId { get; set; }


        public string TemplateRightsId { get; set; }


        public TemplateRights TemplateRightsRole { get; set; }
    }
}