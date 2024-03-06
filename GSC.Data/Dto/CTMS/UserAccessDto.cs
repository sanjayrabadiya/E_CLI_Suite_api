using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
    public class UserAccessDto : BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ParentProjectId { get; set; }
        public bool IsSite { get; set; }
        public List<SiteUserAccessDto> siteUserAccess { get; set; }
    }
    public class SiteUserAccessDto
    {
        public int ProjectId { get; set; }
        public List<MultiUserAccessDTO> multiUserAccess { get; set; }
    }
    public class MultiUserAccessDTO
    {
        [Required(ErrorMessage = "User is required.")]
        public int UserRoleId { get; set; }
    }
    public class UserAccessGridDto : BaseAuditDto
    {
        public int ParentProjectId { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string SiteCode { get; set; }
        public string Access { get; set; }
        public int LoginUser { get; set; }
        public int projectCreatedBy { get; set; }
        public int UserId { get; set; }
        public int roleId { get; set; }
        public string Role { get; set; }
        public string RoleUser { get; set; }
        public string InactiveSiteCode { get; set; }
    }

    public class UserAccessHistoryDto : BaseDto
    {
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public bool? IsRecordDeleted { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }
        public DateTime? RevokeOn { get; set; }
        public string RevokeBy { get; set; }
        public string RevokeByRole { get; set; }
        public DateTime? GrantOn { get; set; }
        public string GrantBy { get; set; }
        public string GrantByRole { get; set; }
        public string TimeZone { get; set; }
    }
}
