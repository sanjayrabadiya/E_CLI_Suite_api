using GSC.Data.Entities.Common;
using GSC.Helper.DocumentService;
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

        public int FolderType { get; set; }
        public int ProjectId { get; set; }
        public int CompanyId { get; set; }
        public string FileName { get; set; }
        public string CompanyName { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
