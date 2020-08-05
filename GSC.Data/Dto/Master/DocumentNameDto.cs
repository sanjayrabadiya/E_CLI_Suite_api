using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.Master
{
    public class DocumentNameDto : BaseDto
    {
        [Required(ErrorMessage = "Document Type is required.")]
        public int DocumentTypeId { get; set; }

        [Required(ErrorMessage = "Document Name is required.")]
        public string Name { get; set; }

      //  public int? CompanyId { get; set; }

        public DocumentType DocumentType { get; set; }
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        //public int CreatedBy { get; set; }
        //public int? DeletedBy { get; set; }
        //public int? ModifiedBy { get; set; }
        //public DateTime? CreatedDate { get; set; }
        //public DateTime? ModifiedDate { get; set; }
        //public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        //public string CompanyName { get; set; }
    }

    public class DocumentNameGridDto : BaseAuditDto
    {
        public int DocumentTypeId { get; set; }
        public string Name { get; set; }
        public DocumentType DocumentType { get; set; }
    }
}