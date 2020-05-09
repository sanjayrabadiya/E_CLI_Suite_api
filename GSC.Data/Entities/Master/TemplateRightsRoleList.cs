using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class TemplateRightsRoleList : BaseEntity
    {
        [ForeignKey("SecurityRoleId")] public string SecurityRoleId { get; set; }


        //[ForeignKey("TemplateRightsId")]
        public int? TemplateRightsId { get; set; }

        public int? CompanyId { get; set; }

        //public TemplateRights TemplateRightsRole { get; set; }
    }
}