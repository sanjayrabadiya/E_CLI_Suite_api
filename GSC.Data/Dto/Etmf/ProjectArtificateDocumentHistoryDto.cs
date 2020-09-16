using GSC.Data.Entities.Common;
using GSC.Helper.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectArtificateDocumentHistoryDto : BaseDto
    {
        public int ProjectWorkplaceArtificateDocumentId { get; set; }
        public string DocumentName { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
