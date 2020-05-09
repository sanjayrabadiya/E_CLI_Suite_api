using System.Collections.Generic;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Report
{
    public class ProjectDatabaseDto : BaseDto
    {
        public int ScreeningTemplateValueId { get; set; }
        public int? ScreeningTemplateParentId { get; set; }
        public int DesignOrder { get; set; }
        public int DesignOrderOfVariable { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ScreeningEntryId { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string DomainName { get; set; }
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
        public List<ProjectDatabaseDto> LstVariable { get; set; }
        public List<ProjectDatabaseDto> LstProjectDataBase { get; set; }
        public List<ProjectDatabaseDto> LstProjectDataBaseVisit { get; set; }
        public List<ProjectDatabaseDto> LstProjectDataBaseitems { get; set; }
    }

    public class ProjectDatabaseSearchDto : BaseDto
    {
        public int ParentProjectId { get; set; }
        public int[] ProjectId { get; set; }
        public int?[] PeriodIds { get; set; }
        public int?[] SubjectIds { get; set; }
        public int?[] VisitIds { get; set; }
        public int?[] TemplateIds { get; set; }
        public int?[] DomainIds { get; set; }
    }
}