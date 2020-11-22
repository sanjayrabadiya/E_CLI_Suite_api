using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Shared.DocumentService;

namespace GSC.Data.Dto.ProjectRight
{
    public class ProjectDocumentDto : BaseDto
    {
        [Required(ErrorMessage = "Project Name is required.")]
        public int ProjectId { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public FileModel FileModel { get; set; }
        public bool IsReview { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string StudyCode { get; set; }
        //Added by Vipul for Acceess training grid display site and study code
        public string SiteCode { get; set; }

    }
}