using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Attendance
{
    public class NoneRegisterDto : BaseDto
    {
        [Required(ErrorMessage = "Initial Name is required.")]
        public string Initial { get; set; }

        [Required(ErrorMessage = "Screening Number is required.")]
        public string ScreeningNumber { get; set; }

        [Required(ErrorMessage = "DateOfScreening is required.")]
        public DateTime? _DateOfScreening { get; set; }

        public DateTime? DateOfScreening
        {
            get => _DateOfScreening.UtcDate();
            set => _DateOfScreening = value.UtcDate();
        }

        public string RandomizationNumber { get; set; }
        public DateTime? _DateOfRandomization { get; set; }

        public DateTime? DateOfRandomization
        {
            get => _DateOfRandomization.UtcDate();
            set => _DateOfRandomization = value.UtcDate();
        }

        public int AttendanceId { get; set; }
        public int ProjectId { get; set; }
        public int ParentProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public int ProjectDesignPeriodId { get; set; }
       // public int? CompanyId { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }

        public bool IsLocked { get; set; }
    }
}