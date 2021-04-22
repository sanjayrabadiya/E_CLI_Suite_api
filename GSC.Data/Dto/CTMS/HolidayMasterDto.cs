using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class HolidayMasterDto: BaseDto
    {
        public int ProjectId { get; set; }
        public string HolidayName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class HolidayMasterListDto
    {
        public string Label { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

    }
}
