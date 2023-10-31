using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
  public  class UserAccessDto : BaseDto
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

        public string Role { get; set; }
        public string RoleUser { get; set; }
        public string InactiveSiteCode { get; set; }
      
    }

}
