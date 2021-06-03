﻿using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class WeekEndMaster : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public DayType AllWeekOff { get; set; }
        public FrequencyType Frequency { get; set; }
        public bool? IsSite { get; set; }
        public Master.Project Project { get; set; }
    }
}
