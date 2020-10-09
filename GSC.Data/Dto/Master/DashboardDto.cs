using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.Master
{
    public class DashboardDto : BaseDto
    {
        public string TaskInformation { get; set; }
        public object ExtraData { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class DashboardDetailsDto : BaseDto
    {
        public List<DashboardDto> eTMFApproveData { get; set; }
        public List<DashboardDto> eTMFSendData { get; set; }
        public List<DashboardDto> eTMFSendBackData { get; set; }
        public List<DashboardDto> eConsentData { get; set; }
    }
}