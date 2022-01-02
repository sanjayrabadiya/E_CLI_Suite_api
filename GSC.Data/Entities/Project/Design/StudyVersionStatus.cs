using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Project.Design
{
    public class StudyVersionStatus : BaseEntity, ICommonAduit
    {
        public ScreeningPatientStatus PatientStatusId { get; set; }
        public int StudyVerionId { get; set; }

        [ForeignKey("StudyVerionId")]
        public StudyVersion StudyVerion { get; set; }

    }
}
