using GSC.Data.Entities.Common;
using GSC.Data.Entities.LabManagement;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.LabManagement
{
    public class LabManagementUploadDataDto : BaseDto
    {
        public int LabManagementConfigurationId { get; set; }
        public int ParentProjectId { get; set; }
        public int ProjectId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public FileModel FileModel { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public LabManagementUploadStatus LabManagementUploadStatus { get; set; }
        public LabManagementConfiguration LabManagementConfiguration { get; set; }
        public IList<LabManagementUploadExcelData> LabManagementUploadExcelDatas { get; set; } = null;
        public Entities.Master.Project Project { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }
    }

    public class LabManagementUploadDataGridDto : BaseAuditDto
    {
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public string Status { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string ProjectDesignVisitName { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public string MimeType { get; set; }
        public string PathName { get; set; }
        public string FullPath { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public int SecurityRoleId { get; set; }
        public bool IsDifference { get; set; }
        public IList<LabManagementUploadExcelData> LabManagementUploadExcelDatas { get; set; } = null;
    }

    public class LabManagementEmail
    {
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public string ScreeningNumber { get; set; }
        public string Visit { get; set; }

        public IList<LabManagementEmailDetail> LabManagementEmailDetail { get; set; }
    }

    public class LabManagementEmailDetail
    {
        public string TestName { get; set; }
        public string Result { get; set; }
        public string ReferenceRangeLow { get; set; }
        public string ReferenceRangeHigh { get; set; }
        public string AbnoramalFlag { get; set; }
    }
}
