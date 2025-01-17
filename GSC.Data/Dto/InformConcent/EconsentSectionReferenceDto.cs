﻿using GSC.Data.Entities.Common;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentSectionReferenceDto : BaseAuditDto
    {
        public int EconsentSetupId { get; set; }
        public int SectionNo { get; set; }
        public string ReferenceTitle { get; set; }
        public string FilePath { get; set; }
        public List<FileModel> FileModel { get; set; }
    }

    public class EconsentSectionReferenceDocumentType
    {
        public string type { get; set; }
        public string data { get; set; }
    }

    public class EconsentSectionReferenceDocument
    {
        public string type { get; set; }
        public string data { get; set; }
        public string Title { get; set; }
    }

    public class EconcentSectionRefrenceDetailListDto: BaseAuditDto
    {        
        public string ReferenceTitle { get; set; }
        public string FilePath { get; set; }
    }
}
