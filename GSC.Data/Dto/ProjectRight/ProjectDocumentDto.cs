using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper.DocumentService;

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
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}