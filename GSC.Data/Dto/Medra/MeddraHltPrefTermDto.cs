using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MeddraHltPrefTermDto : BaseDto
    {
        public int MedraConfigId { get; set; }
        public long hlt_code { get; set; }
        public string hlt_name { get; set; }
        public string hlt_whoart_code { get; set; }
        public long? hlt_harts_code { get; set; }
        public string hlt_costart_sym { get; set; }
        public string hlt_icd9_code { get; set; }
        public string hlt_icd9cm_code { get; set; }
        public string hlt_icd10_code { get; set; }
        public string hlt_jart_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
