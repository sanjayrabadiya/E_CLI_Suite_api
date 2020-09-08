using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Helper;
using GSC.Helper.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MedraConfigDto : BaseDto
    {
        public int MedraVersionId { get; set; }
        public int LanguageId { get; set; }
        public string VersionName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }
        public string Password { get; set; }
        public string DictionaryName { get; set; }
        public int? CompanyId { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public int CreatedRole { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public FileModel FileModel { get; set; }
        public MedraVersion MedraVersion { get; set; }
        public MedraLanguage Language { get; set; }
        public SummaryDto Summary { get; set; }
    }

    public class SummaryDto
    {
        public int? Soc { get; set; }
        public int? Hlgt { get; set; }
        public int? Hlt { get; set; }
        public int? Pt { get; set; }
        public int? Llt { get; set; }
    }

    public class SaveFileDto
    {
        public int MedraId { get; set; }
        public string Path { get; set; }
        public FolderType FolderType { get; set; }
        public string Language { get; set; }
        public string Version { get; set; }
        public string RootName { get; set; }
        public FileModel FileModel { get; set; }

    }
}
