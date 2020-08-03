using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateReview : BaseEntity
    {
        public int ScreeningTemplateId { get; set; }
        public int RoleId { get; set; }
        public ScreeningStatus Status { get; set; }
        public short ReviewLevel { get; set; }
        public bool IsRepeat { get; set; }


        [ForeignKey("RoleId")] public SecurityRole Role { get; set; }
    }
}