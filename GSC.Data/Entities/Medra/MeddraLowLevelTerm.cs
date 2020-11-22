using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MeddraLowLevelTerm : BaseEntity
    {
        public int MedraConfigId { get; set; }
        public long llt_code { get; set; }
        public string llt_name { get; set; }
        public long? pt_code { get; set; }
        public string llt_whoart_code { get; set; }
        public long? llt_harts_code { get; set; }
        public string llt_costart_sym { get; set; }
        public string llt_icd9_code { get; set; }
        public string llt_icd9cm_code { get; set; }
        public string llt_icd10_code { get; set; }
        public string llt_currency { get; set; }
        public string llt_jart_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
