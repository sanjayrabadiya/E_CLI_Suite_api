using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class StudyVerionVisitStatus : BaseEntity, ICommonAduit
    {
        public int VisitStatusId { get; set; }
        public int StudyVerionId { get; set; }

        [ForeignKey("StudyVerionId")]
        public StudyVersion StudyVerion { get; set; }

        public VisitStatus VisitStatus { get; set; }
    }
}
