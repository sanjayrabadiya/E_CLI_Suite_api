using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class VariableTemplateDto : BaseDto
    {
        [Required(ErrorMessage = "Template Code is required.")]
        public string TemplateCode { get; set; }

        public string ActivityName { get; set; }

        [Required(ErrorMessage = "Template Name is required.")]
        public string TemplateName { get; set; }

        [Required(ErrorMessage = "Domain Name is required.")]
        public int DomainId { get; set; }

        public bool IsRepeated { get; set; }
        public ActivityMode ActivityMode { get; set; }
        //public int? CompanyId { get; set; }
        public List<VariableTemplateDetailDto> VariableTemplateDetails { get; set; }
        public IList<VariableTemplateNoteDto> Notes { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}