using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Shared.DocumentService;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerDocumentDto : BaseDto
    {
        public int VolunteerId { get; set; }


        public string FileName { get; set; }

        public string PathName { get; set; }

        public byte[] DocumentBinary { get; set; }

        public string MimeType { get; set; }

        public string Note { get; set; }

        public FileModel FileModel { get; set; }

        [Required(ErrorMessage = "Document Type is required.")]
        public int DocumentTypeId { get; set; }

        public string DocumentTypeName { get; set; }
        public List<AuditTrail> Changes { get; set; }

        public int DocumentNameId { get; set; }

        public DocumentName DocumentName { get; set; }
    }
}