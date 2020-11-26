using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Client
{
    public class Client : BaseEntity, ICommonAduit
    {
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public int ClientTypeId { get; set; }
        public int? CompanyId { get; set; }
        public int? UserId { get; set; }
        public int? RoleId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public string Logo { get; set; }

        [ForeignKey("RoleId")] public SecurityRole SecurityRole { get; set; }

        public ClientType ClientType { get; set; }
    }
}