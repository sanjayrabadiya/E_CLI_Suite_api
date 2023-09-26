using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.CTMS
{

    public class StudyPlanResourceDto : BaseDto
    {
        public int StudyPlanTaskId { get; set; }
        public int ResourceTypeId { get; set; }
    }
    public class StudyPlanResourceGridDto : BaseAuditDto
    {
        public string ResourceType { get; set; }
        public string ResourceSubType { get; set; }
    }
}
