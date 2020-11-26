using System;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.ProjectRight
{
    public class ProjectDocumentReview : BaseEntity, ICommonAduit
    {
        public int ProjectDocumentId { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public bool IsReview { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string ReviewNote { get; set; }
        public Master.Project Project { get; set; }
        public ProjectDocument ProjectDocument { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public TrainigType? TrainingType { get; set; }
        public int? TrainerId { get; set; }
        public string TrainingDuration { get; set; }
    }
}