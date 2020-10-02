using System;
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
}