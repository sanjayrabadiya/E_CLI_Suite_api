using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Report.Pdf
{
    public class DossierReportDto
    {
        public string ScreeningNumber { get; set; }
        public string Initial { get; set; }
        public string RandomizationNumber { get; set; }
        public ProjectDetails ProjectDetails { get; set; }
        public List<ProjectDesignPeriodReportDto> Period { get; set; }
    }

    public class ProjectDetails
    {
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public int ClientId { get; set; } 
        public int ProjectDesignId { get; set; }
    }

    public class ProjectDesignPeriodReportDto
    {
        public string DisplayName { get; set; }
        public List<ProjectDesignVisitList> Visit { get; set; }

    }

    public class ProjectDesignVisitList
    {
        public string DisplayName { get; set; }
        public int? DesignOrder { get; set; }
        public List<ProjectDesignTemplatelist> ProjectDesignTemplatelist { get; set; }

    }

    public class ProjectDesignTemplatelist
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public int DesignOrder { get; set; }
        public int? RepeatSeqNo { get; set; }
        //public int ProjectDesignId { get; set; }
        public DomainReportDto Domain { get; set; }
        public List<ProjectDesignTemplateNoteReportDto> TemplateNotes { get; set; }

        public List<ProjectDesignTemplateNoteReportDto> TemplateNotesBottom { get; set; }
        public List<ProjectDesignVariableReportDto> ProjectDesignVariable { get; set; }
        public List<ScreeningTemplateReviewReportDto> ScreeningTemplateReview { get; set; }
      
    }

    public class DomainReportDto
    {
        public string DomainName { get; set; }
        public string DomainCode { get; set; }
    }

    public class ProjectDesignTemplateNoteReportDto
    {
        public bool IsPreview { get; set; }
        public string Notes { get; set; }

        public bool? IsBottom { get; set; }
    }


    public class ProjectDesignVariableReportDto
    {
        public int Id { get; set; }
        public string VariableName { get; set; }
        public string VariableCode { get; set; }
        public int DesignOrder { get; set; }
        public string Annotation { get; set; }
        public string CollectionAnnotation { get; set; }
        public UnitReportDto Unit { get; set; }
        public bool IsNa { get; set; }
        public string Note { get; set; }
        public string DefaultValue { get; set; }
        public string LowRangeValue { get; set; }
        public string HighRangeValue { get; set; }
        public int? LargeStep { get; set; }

        public CollectionSources CollectionSource { get; set; }
        public List<ProjectDesignVariableValueReportDto> Values { get; set; }

        public int ScreeningTemplateValueId { get; set; }
        public string ScreeningValue { get; set; }
        public bool ScreeningIsNa { get; set; }
        public List<ScreeningTemplateValueChildReportDto> ValueChild { get; set; }

        public string VariableCategoryName { get; set; }

        
    }

    public class UnitReportDto
    {
        public string UnitName { get; set; }

        public string UnitAnnotation { get; set; }
    }

    public class ProjectDesignVariableValueReportDto
    {
        public int Id { get; set; }
        public string ValueName { get; set; }
        public int SeqNo { get; set; }
        public string ValueCode { get; set; }
        public string Label { get; set; }
    }

    public class ScreeningTemplateValueChildReportDto
    {
        public int ScreeningTemplateValueId { get; set; }
        public int ProjectDesignVariableValueId { get; set; }
        public string Value { get; set; }
        public string ValueName { get; set; }
    }

    public class ScreeningTemplateReviewReportDto
    {
        public int ScreeningTemplateId { get; set; }
        public short ReviewLevel { get; set; }
        public int RoleId { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string RoleName { get; set; }
    }

  
}
