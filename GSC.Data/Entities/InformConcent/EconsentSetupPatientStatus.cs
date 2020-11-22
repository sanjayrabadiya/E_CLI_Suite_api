using GSC.Common.Base;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentSetupPatientStatus : BaseEntity
    {
        public int EconsentDocumentId { get; set; }

        [ForeignKey("EconsentDocumentId")] 
        public EconsentSetup EconsentSetup { get; set; }

        public int PatientStatusId { get; set; }

        [ForeignKey("PatientStatusId")]
        public PatientStatus PatientStatus { get; set; }
    }
}
