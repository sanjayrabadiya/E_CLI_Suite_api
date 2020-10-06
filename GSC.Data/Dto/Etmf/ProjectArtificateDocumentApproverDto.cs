using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Helper.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectArtificateDocumentApproverDto : BaseDto
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public int UserId { get; set; }
        public bool? IsApproved { get; set; }
        public int CompanyId { get; set; }
    }
}
