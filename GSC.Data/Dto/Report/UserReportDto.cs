using GSC.Data.Entities.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.Report
{
    public class UserReportSearchDto : BaseDto
    {
        public int? ProjectId { get; set; }
        public int UserId { get; set; }
        public int?[] RoleIds { get; set; }
        public int?[] UserIds { get; set; }
        private DateTime? _FromDate;

        private DateTime? _ToDate;
        public DateTime? FromDate
        {
            get => _FromDate.UtcDate();
            set => _FromDate = value.UtcDate();
        }

        public DateTime? ToDate
        {
            get => _ToDate.UtcDate();
            set => _ToDate = value.UtcDate();
        }
    }

    
    public class UserReportDto : BaseDto
    {
        public UserReportDto()
        {
            UserRoles = new List<UserRole>();
        }
        public string UserName { get; set; }
      
        public string CreatedBy { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string RoleName { get; set; }
        public List<UserRole> UserRoles { get; set; }
        public DateTime? LoginTime { get; set; }
        public string LastIpAddress { get; set; }
        public string AuditReasonName { get; set; }
        public DateTime? LogOutTime { get; set; }
        public string Session { get; set; }
        public int? ParentProjectId { get; set; }
        public int? ChildProjectId { get; set; }
        
        public string SiteName { get; set; }
        public UserMasterUserType? UserType { get; set; }

    }

    public class ModuleNameDto : BaseDto
    {
        public int ModuleId { get; set; }
        public int? SubModuleId { get; set; }
        public string ModuleName { get; set; }
        
    }
    public class UserRoleReportSearchDto : BaseDto
    {
        public int? ProjectId { get; set; }
        public int ModuleId { get; set; }
        public int UserId { get; set; }
        public int?[] RoleIds { get; set; }
        public int?[] UserIds { get; set; }
        public DateFormats? FormDate { get; set; }
        public DateFormats? ToDate { get; set; }
    }
}

