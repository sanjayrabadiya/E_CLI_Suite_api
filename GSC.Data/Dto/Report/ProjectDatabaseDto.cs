using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Report
{
    public class ProjectDatabaseDto : BaseDto
    {
        public int? ScreeningTemplateValueId { get; set; }
        public int? ScreeningTemplateParentId { get; set; }
        public int DesignOrder { get; set; }
        public int DesignOrderOfVariable { get; set; }
        public int ScreeningTemplateId { get; set; }

        public int? RepeatSeqNo { get; set; }
        public int ScreeningEntryId { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string DomainName { get; set; }
        public string DomainCode { get; set; }
        public int? DomainId { get; set; }
        public string Visit { get; set; }
        public int VisitId { get; set; }
        public int? RepeatedVisit { get; set; }

        public int VariableId { get; set; }
        public string VariableName { get; set; }
        public int? UnitId { get; set; }
        public string Unit { get; set; }
        public string UnitAnnotation { get; set; }
        public string Annotation { get; set; }
        public int CollectionSource { get; set; }
        public string VariableNameValue { get; set; }
        public string VariableUnit { get; set; }
        public string Initial { get; set; }
        public string SubjectNo { get; set; }
        public string RandomizationNumber { get; set; }
        public int ProjectId { get; set; }
        public int? ParentProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public int? VisitDesignOrder { get; set; }
        public int PeriodId { get; set; }
        public List<ProjectDatabaseDto> LstVariable { get; set; }
        public List<ProjectDatabaseDto> LstProjectDataBase { get; set; }
        public List<ProjectDatabaseDto> LstProjectDataBaseVisit { get; set; }
        public List<ProjectDatabaseDto> LstProjectDataBaseitems { get; set; }
        public string VariableCode { get; set; }
        public string VariableChildValue { get; set; }
    }

    public class ProjectDatabaseDomainDto
    {
        public string DomainName { get; set; }
        public string DomainCode { get; set; }
        public int TemplateId { get; set; }
        public int DesignOrder { get; set; }
        public string UnitAnnotation { get; set; }
        public List<ProjectDatabaseVariableDto> LstVariable { get; set; }
        public List<ProjectDatabaseInitialDto> LstProjectDataBase { get; set; }
    }

    public class ProjectDatabaseTableDto
    {
        public string DomainName { get; set; }
        public string VariableName { get; set; }
        public string DomainCode { get; set; }
        public List<ProjectDatabaseTableValueDto> LstVariable { get; set; }
        public string TableHeader { get; set; }
    }

    public class level
    {
        public short? maxno { get; set; }
        public string initial { get; set; }
    }

        public class ProjectDatabaseTableValueDto
    {
        public short? LevelNo { get; set; }
        public string Value { get; set; }
        public string ProjectCode { get; set; } 
        public string ProjectName { get; set; }
        public string Initial { get; set; }
        public string SubjectNo { get; set; }
        public string RandomizationNumber { get; set; }
        public string Visit { get; set; }
        public int DesignOrder { get; set; }
        public string TemplateName { get; set; }
        public short? MaxLevelNo { get; set; }
        public TableCollectionSource? CollectionSource { get; set; }
    }

    public class ProjectDatabaseVariableDto
    {
        public string VariableCode { get; set; }
        public string VariableName { get; set; }
        public string Annotation { get; set; }
        public string Unit { get; set; }
        public int? UnitId { get; set; }
        public int DesignOrderOfVariable { get; set; }
        public string UnitAnnotation { get; set; }
        public int TemplateId { get; set; }
        public string DomainName { get; set; }
    }

    public class ProjectDatabaseInitialDto
    {
        public string Initial { get; set; }
        public string DomainName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public int? ParentProjectId { get; set; }
        public string ProjectName { get; set; }
        public string SubjectNo { get; set; }
        public string RandomizationNumber { get; set; }
        public List<ProjectDatabaseVisitDto> LstProjectDataBaseVisit { get; set; }
    }

    public class ProjectDatabaseVisitDto
    {
        public int VisitId { get; set; }
        public string Visit { get; set; }
        public int DesignOrder { get; set; }
        public int? VisitDesignOrder { get; set; }
        public int PeriodId { get; set; }
        public string TemplateName { get; set; }
        public List<ProjectDatabaseTemplateDto> LstProjectDataBaseTemplate { get; set; }
        
    }

    public class ProjectDatabaseTemplateDto
    {
        public int VisitId { get; set; }
        public string Visit { get; set; }
        public int TemplateId { get; set; }
        public decimal DesignOrder { get; set; }
        public int? RepeatSeqNo { get; set; }
        public string TemplateName { get; set; }
        public List<ProjectDatabaseItemDto> LstProjectDataBaseitems { get; set; }
    }

    public class ProjectDatabaseItemDto
    {
        public int DesignOrder { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string SubjectNo { get; set; }
        public string Initial { get; set; }
        public int? ScreeningTemplateParentId { get; set; }
        public int? ScreeningTemplateValueId { get; set; }
        public string DomainName { get; set; }
        public string VariableName { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int CollectionSource { get; set; }
        public string VariableNameValue { get; set; }
        public int? UnitId { get; set; }
        public string Unit { get; set; }
        public string Visit { get; set; }
        public int? RepeatSeqNo { get; set; }
    }
    public class ProjectDatabaseSearchDto : BaseDto
    {
        public int ParentProjectId { get; set; }
        public int? SiteId { get; set; }
        public int[] ProjectId { get; set; }
        public int?[] PeriodIds { get; set; }
        public int?[] SubjectIds { get; set; }
        public int?[] VisitIds { get; set; }
        public int?[] TemplateIds { get; set; }
        public int?[] DomainIds { get; set; }
        public int?[] VariableIds { get; set; }
        public bool ExcelFormat { get; set; }
        public int SelectedProject { get; set; }
        public DBDSReportFilter? FilterId { get; set; }
        public DbdsReportType? Type { get; set; }
    }

    public class RepeatTemplateDto
    {
        public int TemplateId { get; set; }
        public int? Parent { get; set; }
        public int Row { get; set; }
    }
    public class MeddraDetails
    {
        public string ProjectCode { get; set; }
        public string SiteCode { get; set; }
        public string DomainCode { get; set; }
        public string ScreeningNumber { get; set; }
        public string RandomizationNumber { get; set; }
        public string Initial { get; set; }
        public string Visit { get; set; }
        public int? RepeatedVisit { get; set; }
        public string TemplateName { get; set; }
        public string VariableAnnotation { get; set; }
        public string VariableTerm { get; set; }
        public string Version { get; set; }
        public string Language { get; set; }
        public string SocCode { get; set; }
        public string SocName { get; set; }
        public string SocAbbrev { get; set; }
        public string PrimaryIndicator { get; set; }
        public string HlgtCode { get; set; }
        public string HlgtName { get; set; }
        public string HltCode { get; set; }
        public string HltName { get; set; }
        public string PtCode { get; set; }
        public string PtName { get; set; }
        public string PtSocCode { get; set; }
        public string LltCode { get; set; }
        public string LltName { get; set; }
        public string LltCurrency { get; set; }

        public string CodedBy { get; set; }
        private DateTime? _codedOn;

        public DateTime? CodedOn
        {
            get => _codedOn?.UtcDate();
            set => _codedOn = value?.UtcDate();
        }
        public int CollectionSource { get; set; }
        public string VariableChildValue { get; set; }
    }

    public class CommonDto
    {
        public List<ProjectDatabaseDomainDto> Dbds { get; set; }
        public List<ProjectDatabaseTableDto> Table { get; set; }
        public List<MeddraDetails> Meddra { get; set; }
    }

    public class RangeOfTemplate
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Visit { get; set; }
        public int FirstCell { get; set; }
        public int LastCell { get; set; }
    }

    public class ReportProjectDesignValue
    {
        [Key]
        public int Id { get; set; }
        public string Value { get; set; }
    }
}