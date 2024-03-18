using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class TemplateRightsRoleList : BaseEntity, ICommonAduit
    {
        [ForeignKey("SecurityRoleId")] public string SecurityRoleId { get; set; }


        public int? TemplateRightsId { get; set; }

        public int? CompanyId { get; set; }
    }
}