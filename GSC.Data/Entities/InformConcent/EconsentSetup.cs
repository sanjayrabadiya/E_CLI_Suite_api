using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentSetup : BaseEntity, ICommonAduit
    {

        public int ProjectId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public string Version { get; set; }
        public int LanguageId { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
        public Language Language { get; set; }
        //public DocumentType DocumentType { get; set; }
        public List<EconsentSetupPatientStatus> PatientStatus { get; set; }
        //public List<EconsentSetupRoles> Roles { get; set; }
        public string OriginalFileName { get; set; }
    }
}
