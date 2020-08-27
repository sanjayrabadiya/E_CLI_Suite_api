using GSC.Data.Entities.Common;
using GSC.Helper.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectArtificateDocumentCommentDto : BaseDto
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public string Comment { get; set; }
        public string Response { get; set; }
        public int? ResponseBy { get; set; }
        public string ResponseByName { get; set; }
        public DateTime? ResponseDate { get; set; }
        public int CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
