using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
  public  class ResourceTypeDto: BaseDto
    {
        [Required(ErrorMessage = "Resource Code is required.")]
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
        public string NameOfMaterial { get; set; }
        public DateTime? BoughtDate { get; set; }
        public string OwnerName { get; set; }
    }

    public class ResourceTypeGridDto: BaseAuditDto
    {
        public int? TaskId { get; set; }
        public string ResourceCode { get; set; }
        public string ResourceType { get; set; }
        public string ResourceSubType { get; set; }
        public string Designation { get; set; }
        public string YersOfExperience { get; set; }
        public string Role { get; set; }
        public string User { get; set; }
        public string Unit { get; set; }
        public string CurrencyType { get; set; }
        public int? NumberOfUnit { get; set; }
        public int? Cost { get; set; }
        public int? TotalCost { get; set; }
        public int? ConvertTotalCost { get; set; }
        public string NameOfMaterial { get; set; }
        public DateTime? BoughtDate { get; set; }
        public string OwnerName { get; set; }
        public string GlobalCurrency { get; set; }
        public decimal? LocalCurrencyRate { get; set; }
        public string LocalCurrencySymbol { get; set; }
        public string GlobalCurrencySymbol { get; set; }
        public int?  UserId { get; set; }
        public int?  SecurityRoleId { get; set; }
    }
    public class ResourceByEdit 
    {
        public int? resourceId { get; set; }
        public int? subresource { get; set; }
        public int? designation { get; set; }
        public int? nameOfMaterial { get; set; }
        public int? rollUser {  get; set; }
        public int? Cost { get; set; }
        public int? NoOfUnit { get; set; }
        public int? TotalCost { get; set; }
        public string Unit { get; set; }
        public int? ConvertTotalCost { get; set; }
        public string ResourceUnit { get; set; }
        public string GlobalCurrency { get; set; }
    }
}
