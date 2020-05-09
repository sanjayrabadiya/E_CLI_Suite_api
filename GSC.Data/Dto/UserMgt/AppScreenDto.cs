using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.UserMgt
{
    public class AppScreenDto : BaseDto
    {
        [Required(ErrorMessage = "Screen Code is required.")]
        public string ScreenCode { get; set; }

        [Required(ErrorMessage = "Screen Name is required.")]
        public string ScreenName { get; set; }

        public int ParentAppScreenId { get; set; }

        public string ParentScreenName { get; set; }
        public bool IsMenu { get; set; }
        public bool IsPermission { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsExport { get; set; }
        public string UrlName { get; set; }
        public int SeqNo { get; set; }
       // public int CompanyId { get; set; }
        public string IconPath { get; set; }
        public bool IsMaster { get; set; }
        public string TableName { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}