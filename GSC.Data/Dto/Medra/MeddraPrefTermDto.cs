using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MeddraPrefTermDto : BaseDto
    {
        public int MedraConfigId { get; set; }
        public long pt_code { get; set; }
        public string pt_name { get; set; }
        public string null_field { get; set; }
        public long? pt_soc_code { get; set; }
        public string pt_whoart_code { get; set; }
        public long? pt_harts_code { get; set; }
        public string pt_costart_sym { get; set; }
        public string pt_icd9_code { get; set; }
        public string pt_icd9cm_code { get; set; }
        public string pt_icd10_code { get; set; }
        public string pt_jart_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
