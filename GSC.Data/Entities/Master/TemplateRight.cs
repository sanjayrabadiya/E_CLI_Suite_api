using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class TemplateRights : BaseEntity
    {
        public string TemplateCode { get; set; }

        public string RoleId { get; set; }
        public int VariableTemplateId { get; set; }
        public int? CompanyId { get; set; }

        public VariableTemplate VariableTemplate { get; set; }
        //public List<TemplateRightsRoleList> TemplateRightsRoleList { get; set; }

        //public SecurityRole SecurityRole { get; set; }
    }
}