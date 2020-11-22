using GSC.Data.Entities.Common;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectSubSecArtificateDocumentCommentDto : BaseDto
    {
        public int ProjectWorkplaceSubSecArtificateDocumentId { get; set; }
        public string Comment { get; set; }
        public string Response { get; set; }
        public int? ResponseBy { get; set; }
        public string ResponseByName { get; set; }
        public DateTime? ResponseDate { get; set; }
        public bool ViewDelete { get; set; }
        public bool ViewClose { get; set; }
        public bool IsClose { get; set; }
        public int? CloseBy { get; set; }
        public DateTime? CloseDate { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
