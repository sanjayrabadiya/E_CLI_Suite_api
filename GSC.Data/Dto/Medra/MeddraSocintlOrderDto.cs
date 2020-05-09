using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MeddraSocIntlOrderDto : BaseDto
    {
        public int MedraConfigId { get; set; }
        public long intl_ord_code { get; set; }
        public long soc_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
