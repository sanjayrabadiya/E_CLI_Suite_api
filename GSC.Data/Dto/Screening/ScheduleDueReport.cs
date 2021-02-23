using GSC.Data.Entities.Common;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Screening
{
    public class ScheduleDueReport
    {
        public Int64 Id { get; set; }
        public string studyCode { get; set; }
        public string siteCode { get; set; }
        public string initial { get; set; }
        public string screeningNo { get; set; }
        public string randomizationNumber { get; set; }
        public string visitName { get; set; }
        public string templateName { get; set; }
        public DateTime? scheduleDate { get; set; }
        public string? scheduleDateExcel { get; set; }
        //private DateTime? _scheduleDate { get; set; }
        //public DateTime? scheduleDate
        //{
        //    get => _scheduleDate.UtcDate();
        //    set => _scheduleDate = value.UtcDate();
        //}

    }

    public class ScheduleDueReportSearchDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int?[] SubjectIds { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        //private DateTime? _fromDate { get; set; }
        //public DateTime? fromDate
        //{
        //    get => _fromDate.UtcDate();
        //    set => _fromDate = value.UtcDate();
        //}
        //private DateTime? _toDate { get; set; }
        //public DateTime? toDate
        //{
        //    get => _toDate.UtcDate();
        //    set => _toDate = value.UtcDate();
        //}
    }
}
