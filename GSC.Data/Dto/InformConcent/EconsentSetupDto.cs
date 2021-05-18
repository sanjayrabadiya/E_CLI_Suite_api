using GSC.Data.Entities.Common;
using GSC.Data.Entities.InformConcent;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentSetupDto : BaseDto
    {
        public EconsentSetupDto()
        {
            PatientStatus = new List<EconsentSetupPatientStatus>();
            Roles = new List<EconsentSetupRoles>();
        }
        public int ProjectId { get; set; }
        //public int DocumentTypeId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public string Version { get; set; }
        public int LanguageId { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
        public Language Language { get; set; }
        public string LanguageName { get; set; }
        public string ProjectName { get; set; }
        public FileModel FileModel { get; set; }
        public List<EconsentSetupPatientStatus> PatientStatus { get; set; }
        public List<EconsentSetupRoles> Roles { get; set; }
        public string OriginalFileName { get; set; }
    }

    public class SaveFileDto
    {
        public string Path { get; set; }
        public FolderType FolderType { get; set; }
        public string RootName { get; set; }
        public FileModel FileModel { get; set; }

    }

    public class EconsentSetupGridDto : BaseAuditDto
    {
        
        //public int Id { get; set; }
        public int ProjectId { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public string Version { get; set; }
        public int LanguageId { get; set; }
        public string LanguageName { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProjectName { get; set; }
        public string PatientStatusData { get; set; }
        public string ApproveBy { get; set; }

    }

}
