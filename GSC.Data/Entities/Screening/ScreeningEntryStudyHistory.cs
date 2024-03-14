using GSC.Common.Base;
using System.ComponentModel.DataAnnotations.Schema;

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


    }
}
