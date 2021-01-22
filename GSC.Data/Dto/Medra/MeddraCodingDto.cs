using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using GSC.Shared.Extension;
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
        public CodedType? CodedType { get; set; }
        public CodedType? CodingType { get; set; }
        public int? ApprovedBy { get; set; }
        public bool? IsApproved { get; set; }
        public int? CompanyId { get; set; }
        private DateTime? _modifiedDate;

        public DateTime? ModifiedDate
        {
            get => _modifiedDate.UtcDateTime();
            set => _modifiedDate = value?.UtcDateTime();
        }
        public int ModifiedBy { get; set; }
        public int CreatedRole { get; set; }
        public int?[] ScreeningTemplateValueIds { get; set; }

        public int? TemplateStatus { get; set; }
        public bool? ExtraData { get; set; }
        private DateTime? _approveDate;
        public DateTime? ApproveDate
        {
            get => _approveDate?.UtcDateTime();
            set => _approveDate = value?.UtcDateTime();
        }
    }

    public class MeddraCodingSearchDto
    {
        public int? Id { get; set; }
        public int MeddraConfigId { get; set; }
        private DateTime? _FromDate;
        public DateTime? FromDate
        {
            get => _FromDate.UtcDate();
            set => _FromDate = value.UtcDate();
        }
        private DateTime? _ToDate;
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
        public CodedType? Status { get; set; }
        public CodedType? CodingStatus { get; set; }
        public string Value { get; set; }
        public int SearchBy { get; set; }
        public int? TemplateStatus { get; set; }
        public int?[] SubjectIds { get; set; }
        public int? CommentStatus { get; set; }
        public bool? ExtraData { get; set; }
        public bool? IsApproved { get; set; }
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
        public string ModifiedByRole { get; set; }

        public bool IsCoding { get; set; }
        public bool IsApproveProfile { get; set; }
        public bool IsShow { get; set; }
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
        public string UpdatedByRole { get; set; }

        private DateTime? _lastUpdateOn;

        public DateTime? LastUpdateOn
        {
            get => _lastUpdateOn?.UtcDateTime();
            set => _lastUpdateOn = value?.UtcDateTime();
        }

        public int SocId { get; set; }
        public string SocCode { get; set; }
        public long LLT { get; set; }
        public string LLTValue { get; set; }
        public string PT { get; set; }
        public string HLT { get; set; }
        public string HLGT { get; set; }
        public string SOCValue { get; set; }
        public string PrimarySoc { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public int MeddraConfigId { get; set; }
        public int MeddraLowLevelTermId { get; set; }
        public int MeddraSocTermId { get; set; }
        public CodedType? CodedType { get; set; }
        public CodedType? CodingType { get; set; }
        public string SiteCode { get; set; }
        public int? MeddraCodingId { get; set; }
        public CommentStatus? CommentStatus { get; set; }
        public bool? IsApproved { get; set; }
        public string LltCurrent { get; set; }
    }
}
