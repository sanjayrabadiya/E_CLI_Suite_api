using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;

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
        public DashboardMyTaskType ControlType { get; set; }

        public int CtmsMonitoringId { get; set; }

        public int ActivityId { get; set; }
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
        public List<DashboardDto> eAdverseEventData { get; set; }
        public List<DashboardDto> manageMonitoringReportSendData { get; set; }
        public List<DashboardDto> manageMonitoringReportSendBackData { get; set; }
    }

    public class DashboardMyTaskDto : BaseDto
    {
        public List<DashboardDto> MyTaskList { get; set; }
    }

    public class Dataset
    {
        public List<int> Data { get; set; }
        public string Label { get; set; }
    }

    public class DashboardGraph
    {
        public List<string> Labels { get; set; }
        public List<Dataset> Datasets { get; set; }
    }

    public class DashboardDaysScreenedToRandomized
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public decimal AvgDayDiff { get; set; }
    }

    public class RandomizedProgress
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int Progress { get; set; }
    }

    public class LabelGraph
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int RandomizedCount { get; set; }
        public int ScreeningCount { get; set; }
        public int TargetCount { get; set; }
    }

    public class FormsGraphModel
    {
        public string StatusName { get; set; }
        public int RecordCount { get; set; }
    }

    public class GraphModel
    {
        public string X { get; set; }
        public int Y { get; set; }
    }

    public class CloumnGraph
    {
        public string Name { get; set; }
        public List<GraphModel> RecordCount { get; set; }
    }

    public class CtmsTaskDue
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
    }
}