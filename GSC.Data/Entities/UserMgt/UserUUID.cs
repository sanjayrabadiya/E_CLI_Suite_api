using GSC.Common.Base;
using GSC.Helper;

namespace GSC.Data.Entities.UserMgt
{
    public class UserUUID : BaseEntity
    {
        public int UserId { get; set; }
        public string UUID { get; set; }
        public bool Active { get; set; }
        public DeviceType DeviceType { get; set; }
        public string StudyCode { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
    }
}
