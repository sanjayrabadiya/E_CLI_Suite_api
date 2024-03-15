using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.Screening
{
    public class VisitDeviationReport
    {
        [Key]
        public Int64 Id { get; set; }
        public string Initial { get; set; }
        public string ScreeningNo { get; set; }
        public DateTime ScreeningDate { get; set; }
        public string RefVisit { get; set; }
        public string RefTemplate { get; set; }
        public string RefVariable { get; set; }
        public DateTime RefValue { get; set; }
        public string TargetVisit { get; set; }
        public string TargetTemplate { get; set; }
        public string TargetVariable { get; set; }
        public DateTime TargetValue { get; set; }
        public int NoOfDay { get; set; }
        public int HH { get; set; }
        public int MM { get; set; }
        public int PositiveDeviation { get; set; }
        public int NegativeDeviation { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public int ProjectId { get; set; }
        public string SiteCode { get; set; }
        public string StudyCode { get; set; }
        public int RandomizationId { get; set; }
        public string Unit { get; set; }
        public Int32? Deviation { get; set; }
        public string RefValueExcel { get; set; }
        public string TargetValueExcel { get; set; }
        public string RandomizationNumber { get; set; }
    }

    public class VisitDeviationReportSearchDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int?[] PeriodIds { get; set; }
        public int?[] SubjectIds { get; set; }
        public int?[] VisitIds { get; set; }
        public int?[] TemplateIds { get; set; }
        public int?[] VariableIds { get; set; }
    }
}
