using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;

namespace GSC.Data.Entities.ProjectRight
{
    public class ProjectDocument : BaseEntity
    {
        public int ProjectId { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public Master.Project Project { get; set; }

        [NotMapped] public bool IsReview { get; set; }
    }
}