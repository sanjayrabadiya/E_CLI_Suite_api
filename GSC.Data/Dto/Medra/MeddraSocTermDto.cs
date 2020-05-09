using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MeddraSocTermDto : BaseDto
    {
        public int MedraConfigId { get; set; }
        public long soc_code { get; set; }
        public string soc_name { get; set; }
        public string soc_abbrev { get; set; }
        public long? soc_harts_code { get; set; }
        public string soc_costart_sym { get; set; }
        public string soc_icd9_code { get; set; }
        public string soc_icd9cm_code { get; set; }
        public string soc_icd10_code { get; set; }
        public string soc_jart_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
