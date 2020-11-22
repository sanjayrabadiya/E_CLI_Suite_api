using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateValueComment : BaseEntity
    {
        public int ScreeningTemplateValueId { get; set; }
        public int? RoleId { get; set; }
        public string Comment { get; set; }
        public ScreeningTemplateValue ScreeningTemplateValue { get; set; }

        [ForeignKey("RoleId")] public SecurityRole Role { get; set; }
    }
}