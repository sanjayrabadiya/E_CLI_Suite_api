using System;
using System.Collections.Generic;
using GSC.Data.Dto.Master;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Screening
{
    //public class ScreeningTemplateLockUnlockParams
    //{
    //    public int? ProjectDesignVisitId { get; set; }
    //    public int ProjectId { get; set; }
    //    public int? ProjectDesingId { get; set; }
    //    public int? VolunteerId { get; set; }
    //    public bool IsLock { get; set; }
    //    public List<int> Ids { get; set; }
    //    public ScreeningTemplateStatus Status { get; set; }
    //}

    public class ScreeningTemplateLockUnlockDto
    {
        public int Id { get; set; }
        public string VolunteerName { get; set; }
        public string ScreeningNo { get; set; }
        public string ProjectName { get; set; }
        public string VisitName { get; set; }
        public string TemplateName { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public string StatusName { get; set; }
    }

    public class LockUnlockSearchDto
    {
        public int ParentProjectId { get; set; }
        public int ProjectId { get; set; }
        public int?[] PeriodIds { get; set; }
        public int?[] SubjectIds { get; set; }
        public int?[] VisitIds { get; set; }
        public int?[] TemplateIds { get; set; }
        public int?[] DataEntryStatus { get; set; }
        public int?[] DataEntryReviewStatus { get; set; }
        public bool Status { get; set; }
    }

    public class LockUnlockDDDto
    {
        public int?[] Id { get; set; }
        public int ProjectId { get; set; }
        public int ChildProjectId { get; set; }
        public int?[] SubjectIds { get; set; }
        public bool IsLock { get; set; }
    }

    public class ScreeningTemplateLockUnlockAuditDto
    {
        public int ScreeningEntryId { get; set; }
        public int Id { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ProjectId { get; set; }
        public int AuditReasonId { get; set; }
        public string AuditReasonComment { get; set; }
        public bool IsLocked { get; set; }

    }

    public class LockUnlockListDto
    {
        public int Id { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public int? ScreeningTemplateParentId { get; set; }
        public string VolunteerName { get; set; }
        public string ScreeningNo { get; set; }
        public string ProjectName { get; set; }
        public string VisitName { get; set; }
        public string PeriodName { get; set; }
        public string TemplateName { get; set; }
        public bool Status { get; set; }
        public string StatusName { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int screeningEntryId { get; set; }
        public int TemplateId { get; set; }
        public string DomainName { get; set; }
        public int VisitId { get; set; }
        public int VariableId { get; set; }
        public string Annotation { get; set; }
        public int CollectionSource { get; set; }
        public string Initial { get; set; }
        public string SubjectNo { get; set; }
        public string RandomizationNumber { get; set; }
        public string ProjectCode { get; set; }
        public int TemplateCount { get; set; }
        public int VisitCount { get; set; }
        public int PeriodCount { get; set; }
        public List<LockUnlockListDto> lstTemplate { get; set; }
        public bool IsElectronicSignature { get; set; }
        public bool IsLocked { get; set; }
        public int ProjectDesignId { get; set; }
        public int ProjectId { get; set; }
        public int? ParentProjectId { get; set; }
        public ScreeningTemplateStatus ScreeningStatusNo { get; set; }
        public short? ReviewLevel { get; set; }
        public string DesignOrder { get; set; }
        public int SeqNo { get; set; }

    }

    public class LockUnlockHistoryListDto
    {
        public int ScreeningEntryId { get; set; }
        public int Id { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public string VisitName { get; set; }
        public string PeriodName { get; set; }
        public int ProjectId { get; set; }
        public int? ParentProjectId { get; set; }
        public string ProjectName { get; set; }
        public int ProjectDesignId { get; set; }
        public int AuditReasonId { get; set; }
        public string AuditReasonName { get; set; }
        public string AuditReasonComment { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public bool IsLocked { get; set; }
        public string Locked { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedRoleBy { get; set; }
        public string CreatedRoleByName { get; set; }
        public string ProjectCode { get; set; }

        private DateTime? _createdDate;
        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }
        public string VolunteerName { get; set; }
        public string VolunteerNumber { get; set; }
        public string RandomizationNumber { get; set; }
        public int AttendanceId { get; set; }
        public string DesignOrder { get; set; }

        public int? ScreeningTemplateParentId { get; set; }
        public int ScreeningTemplateId { get; set; }

        public int SeqNo { get; set; }
        public int VisitId { get; set; }
    }
}