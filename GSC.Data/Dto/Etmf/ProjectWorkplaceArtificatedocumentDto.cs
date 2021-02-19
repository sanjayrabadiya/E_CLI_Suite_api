using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectWorkplaceArtificatedocumentDto : BaseDto
    {
        public int ProjectWorkplaceArtificateId { get; set; }
        public string DocumentName { get; set; }
        public FileModel FileModel { get; set; }
        public string DocPath { get; set; }
        public string Projectname { get; set; }
        public string Countryname { get; set; }
        public string Sitename { get; set; }
        public string Zonename { get; set; }
        public string Sectionname { get; set; }
        public string Artificatename { get; set; }
        public bool? IsAccepted { get; set; }
        public int FolderType { get; set; }
        public int ProjectId { get; set; }
        public int CompanyId { get; set; }
        public string FileName { get; set; }
        public ArtifactDocStatusType Status { get; set; }
        public string Version { get; set; }
        public int? ParentDocumentId { get; set; }
        public bool SuperSede { get; set; }
        
    }
}
