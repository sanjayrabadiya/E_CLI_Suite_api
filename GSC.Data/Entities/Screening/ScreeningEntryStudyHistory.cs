using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningEntryStudyHistory : BaseEntity
    {
        public int ScreeningEntryId { get; set; }
        public int StudyId { get; set; }
        public int? RoleId { get; set; }
        public string Notes { get; set; }
        public ScreeningEntry ScreeningEntry { get; set; }

        [ForeignKey("StudyId")] 
        public Master.Project Study { get; set; }

       // public SecurityRole SecurityRole { get; set; }

    }
}
