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
        public string Module { get; set; }
        public object ExtraData { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedByUser { get; set; }
        public string DataType { get; set; }
        public double Level { get; set; }
        public int DocumentId { get; set; }
        public int VariableTemplateId { get; set; }
    }

    public class DashboardDetailsDto : BaseDto
    {
        public List<DashboardDto> eTMFApproveData { get; set; }
        public List<DashboardDto> eTMFSubSecApproveData { get; set; }
        public List<DashboardDto> eTMFSendData { get; set; }
        public List<DashboardDto> eTMFSubSecSendData { get; set; }
        public List<DashboardDto> eTMFSendBackData { get; set; }
        public List<DashboardDto> eTMFSubSecSendBackData { get; set; }
        public List<DashboardDto> eConsentData { get; set; }
        public List<DashboardDto> manageMonitoringReportSendData { get; set; }
        public List<DashboardDto> manageMonitoringReportSendBackData { get; set; }
    }
}