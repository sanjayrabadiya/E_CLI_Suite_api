using GSC.Data.Entities.Common;
using GSC.Data.Entities.LabManagement;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.UserMgt;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.LabManagement
{
    public class LabManagementConfigurationDto : BaseDto
    {
        public int ParentProjectId { get; set; }
        public int? ProjectId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public FileModel FileModel { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public int SecurityRoleId { get; set; }
        public int?[] UserIds { get; set; }
        public Entities.Master.Project Project { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public List<LabManagementSendEmailUser> LabManagementSendEmailUser { get; set; }
    }

    public class LabManagementConfigurationGridDto : BaseAuditDto
    {
        public int ParentProjectId { get; set; }
        public int? ProjectId { get; set; }
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string ProjectDesignVisitName { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public string MimeType { get; set; }
        public string ApproveProfile { get; set; }
    }

    public class LabManagementConfigurationEdit
    {
        public int Id { get; set; }
       // public int SecurityRoleId { get; set; }
        public int?[] UserIds { get; set; }
    }
}
