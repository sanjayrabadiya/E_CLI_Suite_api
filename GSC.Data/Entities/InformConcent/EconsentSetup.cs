using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentSetup : BaseEntity
    {

        //public int Id { get; set; }
        public int ProjectId { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public string Version { get; set; }
        public int LanguageId { get; set; }
        //public int PatientStatusId { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
        public Language Language { get; set; }
        public DocumentType DocumentType { get; set; }
        public List<EconsentSetupPatientStatus> PatientStatus { get; set; } = null;
        //public PatientStatus PatientStatus { get; set; }
    }
}
