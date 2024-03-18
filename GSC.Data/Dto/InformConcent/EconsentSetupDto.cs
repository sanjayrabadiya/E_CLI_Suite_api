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
      
        public int ProjectId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public string Version { get; set; }
        public int LanguageId { get; set; }
        public string LanguageName { get; set; }
        public string ProjectName { get; set; }
        //temp open remove this
        public FileModel FileModel { get; set; }
        public string OriginalFileName { get; set; }
        public string IntroVideoPath { get; set; }
        public FileModel IntroVideo { get; set; }
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

        public int ProjectId { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public string Version { get; set; }
        public int LanguageId { get; set; }
        public string LanguageName { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProjectName { get; set; }
        public DocumentStatus DocumentStatusId { get; set; }
        public string DocumentStatus { get; set; }
        public string IntroVideoPath { get; set; }

    }


}
