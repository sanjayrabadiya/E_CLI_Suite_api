using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Master
{
    public class SiteTeamDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string ContactEmail { get; set; }
        public string ContactMobile { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
    public class SiteTeamGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string ContactEmail { get; set; }
        public string ContactMobile { get; set; }
    }
}
