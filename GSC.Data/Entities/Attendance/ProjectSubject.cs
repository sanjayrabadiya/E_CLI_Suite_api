using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Attendance
{
    public class ProjectSubject : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int ParentProjectId { get; set; }
        public int? VolunteerId { get; set; }
        public string Number { get; set; }
        public int? RefProjectSubjectId { get; set; }
        public SubjectNumberType NumberType { get; set; }
        public bool IsRepaced { get; set; }
        public bool IsTesting { get; set; }
    }
}