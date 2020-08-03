using System;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.ProjectRight
{
    public class ProjectDocumentReview : BaseEntity
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