using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MeddraHltPrefCompDto : BaseDto
    {
        public int MedraConfigId { get; set; }
        public long hlt_code { get; set; }
        public long pt_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
