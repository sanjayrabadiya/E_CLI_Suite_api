using GSC.Data.Entities.Common;
using GSC.Helper.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentSectionReferenceDto : BaseDto
    {
        //public int Id { get; set; }
        public int EconsentDocId { get; set; }
        public int SectionNo { get; set; }
        public string ReferenceTitle { get; set; }
        public string FilePath { get; set; }
        public FileModel FileModel { get; set; }
    }

    public class EconsentSectionReferenceDocumentType
    {
        public string type { get; set; }
        public string data { get; set; }
    }
}
