using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Data.Entities.Location;

namespace GSC.Data.Dto.Attendance
{
    public class NoneRegisterDto : BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Middle Name is required.")]
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Initial Name is required.")]
        public string Initial { get; set; }
        [Required(ErrorMessage = "Initial Name is required.")]
        public DateTime? DateOfBirth { get; set; }
        [Required(ErrorMessage = "Gender is required.")]
        public int Gender { get; set; }

        public string PrimaryContactNumber { get; set; }
        public string EmergencyContactNumber { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Initial Name is required.")]
        public string Qualification { get; set; }
        [Required(ErrorMessage = "Occupation is required.")]
        public string Occupation { get; set; }
        [Required(ErrorMessage = "Language is required.")]
        public int LanguageId { get; set; }
        [Required(ErrorMessage = "Address is required.")]
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        [Required(ErrorMessage = "City is required.")]
        public int CityId { get; set; }
        public int ZipCode { get; set; }
        public string LegalFirstName { get; set; }
        public string LegalMiddleName { get; set; }
        public string LegalLastName { get; set; }
        public string LegalEmergencyCoNumber { get; set; }
        public string LegalEmail { get; set; }
        public int LegalRelationship { get; set; }
        public bool LegalStatus { get; set; }

        public string CityName { get; set; }
        // public int? CompanyId { get; set; }
        public City City { get; set; }

        public int StateId { get; set; }

        public int CountryId { get; set; }

        public string StateName { get; set; }

        public string CountryName { get; set; }
        public string ScreeningNumber { get; set; }
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
        //public int ProjectId { get; set; }
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