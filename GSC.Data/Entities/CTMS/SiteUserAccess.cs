using GSC.Common.Base;
using GSC.Common.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace GSC.Data.Entities.CTMS
{
    public class SiteUserAccess : BaseEntity, ICommonAduit
    {
        public int? ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Master.Project Project { get; set; }

        public int UserAccessId { get; set; }
        [ForeignKey("UserAccessId")]
        public UserAccess UserAccess { get; set; }
    }

}
