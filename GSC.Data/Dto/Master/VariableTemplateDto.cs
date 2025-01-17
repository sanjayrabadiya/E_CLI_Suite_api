﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class VariableTemplateDto : BaseDto
    {
        [Required(ErrorMessage = "Template Code is required.")]
        public string TemplateCode { get; set; }


        [Required(ErrorMessage = "Template Name is required.")]
        public string TemplateName { get; set; }

        [Required(ErrorMessage = "Domain Name is required.")]
        public int DomainId { get; set; }

        public bool IsRepeated { get; set; }
        public ActivityMode ActivityMode { get; set; }
        public int? AppScreenId { get; set; }
        public List<VariableTemplateDetailDto> VariableTemplateDetails { get; set; }
        public IList<VariableTemplateNoteDto> Notes { get; set; }
        public int? CompanyId { get; set; }
        public string DomainCode { get; set; }
    }

    public class VariableTemplateGridDto : BaseAuditDto
    {
        public string TemplateCode { get; set; }
        public string ModuleName { get; set; }
        public string DomainName { get; set; }
        public string ActivityMode { get; set; }
        public string TemplateName { get; set; }
        public bool IsRepeated { get; set; }

    }
}