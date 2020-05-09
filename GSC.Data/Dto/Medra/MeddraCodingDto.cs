using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MeddraCodingDto : BaseDto
    {
        public int MeddraConfigId { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public int MeddraLowLevelTermId { get; set; }
        public int? MeddraSocTermId { get; set; }
        public CodedType CodedType { get; set; }
        public int? ApprovedBy { get; set; }
        public bool IsApproved { get; set; }
        public int? CompanyId { get; set; }
        private DateTime? _modifiedDate;

        public DateTime? ModifiedDate
        {
            get => _modifiedDate.UtcDateTime();
            set => _modifiedDate = value?.UtcDateTime();
        }
        public int ModifiedBy { get; set; }
    }

    public class MeddraCodingSearchDto
    {
        public int MeddraConfigId { get; set; }
        private DateTime? _FromDate;

        private DateTime? _ToDate;
        public int? Id { get; set; }

        public DateTime? FromDate
        {
            get => _FromDate.UtcDate();
            set => _FromDate = value.UtcDate();
        }

        public DateTime? ToDate
        {
            get => _ToDate.UtcDate();
            set => _ToDate = value.UtcDate();
        }
        public int? ProjectId { get; set; }
        public int ProjectDesignId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int? CountryId { get; set; }
        public int? SiteId { get; set; }
        public int? Status { get; set; }
        public string Value { get; set; }
        public int SearchBy { get; set; }
        public int? TemplateStatus { get; set; }
        public int?[] SubjectIds { get; set; }
        public int? CommentStatus { get; set; }
    }

    public class MeddraCodingMainDto
    {
        public int All { get; set; }
        public int? CodedData { get; set; }
        public int? ApprovalData { get; set; }
        private DateTime? _ModifiedDate;
        public DateTime? ModifiedDate
        {
            get => _ModifiedDate.UtcDate();
            set => _ModifiedDate = value.UtcDate();
        }
        public string ModifiedBy { get; set; }
    }

    public class MeddraCodingVariableDto
    {
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string VariableName { get; set; }
        public string VariableCode { get; set; }
        public string VariableAlias { get; set; }
        public string TemplateName { get; set; }
        public string VisitName { get; set; }
        public string PeriodName { get; set; }
        public int MeddraConfigId { get; set; }
    }

    public class MeddraCodingSearchDetails
    {
        public int TempId { get; set; }
        public string SubjectId { get; set; }
        public string VisitName { get; set; }
        public string TemplateName { get; set; }
        public string Value { get; set; }
        public string Code { get; set; }
        public bool CodeApplied { get; set; }
        public bool Status { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? LastUpdateOn { get; set; }
        public int SocId { get; set; }
        public long SocCode { get; set; }
        public int LLT { get; set; }
        public string LLTValue { get; set; }
        public string PT { get; set; }
        public string HLT { get; set; }
        public string HLGT { get; set; }
        public string SOCValue { get; set; }
        public string PrimarySoc { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public int MeddraConfigId { get; set; }
        public int MeddraLowLevelTermId { get; set; }

        public CodedType? CodedType { get; set; }
    }
}
