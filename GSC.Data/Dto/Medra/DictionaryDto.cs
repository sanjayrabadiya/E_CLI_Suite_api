using System;
using System.Collections.Generic;
using System.Text;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Medra
{
    public class DictionaryDto: BaseDto
    {
        public string DictionaryCode { get; set; }
        public string DictionaryName { get; set; }
        public int? CompanyId { get; set; }
    }
}
