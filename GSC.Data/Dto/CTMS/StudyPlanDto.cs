﻿using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace GSC.Data.Dto.CTMS
{
    public class StudyPlanDto: BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Template is required.")]
        public int TaskTemplateId { get; set; }
        [Required(ErrorMessage = "Start Date is required.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End Date is required.")]
        public DateTime EndDate { get; set; }
        public int? CurrencyId {  get; set; }  
        public List<CurrencyRateDTO> CurrencyRateList { get; set; }
    }
    public class CurrencyRateDTO
    {
        public int? localCurrencyId { get; set; }
        public decimal? localCurrencyRate { get; set; }
    }
}
