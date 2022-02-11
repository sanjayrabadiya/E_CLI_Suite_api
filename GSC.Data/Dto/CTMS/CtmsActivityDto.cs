using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsActivityDto : BaseDto
    {
        public string ActivityCode { get; set; }
        public string ActivityName { get; set; }
    }
}
