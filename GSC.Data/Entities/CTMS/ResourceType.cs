using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System;

namespace GSC.Data.Entities.CTMS
{
    public class ResourceType : BaseEntity, ICommonAduit
    {
        public string ResourceCode { get; set; }
        public ResourceTypeEnum ResourceTypes { get; set; }
        public SubResourceType ResourceSubType { get; set; }
        public int? RoleId { get; set; }
        public int? UserId { get; set; }
        public int? DesignationId { get; set; }
        public int? UnitId { get; set; }
        public int? NumberOfUnit { get; set; }
        public int? CurrencyId { get; set; }
        public int? Cost { get; set; }
        public DateTime? BoughtDate { get; set; }
        public string NameOfMaterial { get; set; }
        public string OwnerName { get; set; }
        public string ContractorName { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public SecurityRole Role { get; set; }
        public User User { get; set; }
        public Unit Unit { get; set; }
        public Designation Designation { get; set; }
        public Currency Currency { get; set; }
    }
}
