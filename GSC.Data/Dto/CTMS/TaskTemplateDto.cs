﻿using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class TaskTemplateDto : BaseDto
    {
        [Required(ErrorMessage = "Template Code is required.")]
        public string TemplateCode { get; set; }
        [Required(ErrorMessage = "Template Name is required.")]
        public string TemplateName { get; set; }
    }

    public class TaskTemplateGridDto : BaseAuditDto
    {
        public int Id { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }

    }
}
