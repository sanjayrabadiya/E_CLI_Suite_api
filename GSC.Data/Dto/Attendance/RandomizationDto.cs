using System;
using GSC.Data.Entities.Common;
using GSC.Helper;
using System.Collections.Generic;
using GSC.Data.Dto.InformConcent;
using GSC.Shared.Extension;
using GSC.Data.Entities.InformConcent;

namespace GSC.Data.Dto.Attendance
{
    public class RandomizationDto : BaseDto
    {
        public int ProjectId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Initial { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }
        public string PrimaryContactNumber { get; set; }
        public string EmergencyContactNumber { get; set; }
        public string Email { get; set; }
        public string Qualification { get; set; }
        public string Occupation { get; set; }
        public int? LanguageId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public int? CityId { get; set; }
        public int? ZipCode { get; set; }
        public string LegalFirstName { get; set; }
        public string LegalMiddleName { get; set; }
        public string LegalLastName { get; set; }
        public string LegalEmergencyCoNumber { get; set; }
        public string LegalEmail { get; set; }
        public Relationship? LegalRelationship { get; set; }
        public bool LegalStatus { get; set; }
        public int? StateId { get; set; }
        public int? CountryId { get; set; }
        public string ScreeningNumber { get; set; }
        public DateTime? DateOfScreening { get; set; }

        public string RandomizationNumber { get; set; }
        public DateTime? DateOfRandomization { get; set; }

        public int ParentProjectId { get; set; }
        public int? CompanyId { get; set; }
        public bool IsLocked { get; set; }
        public int? UserId { get; set; }
    }

    public class RandomizationGridDto : BaseAuditDto
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Initial { get; set; }
        public DateTime? DateOfBirth { get; set; }
        //public DateTime? DateOfBirth
        //{
        //    get => _DateOfBirth.UtcDate();
        //    set => _DateOfBirth = value.UtcDate();
        //}

        public string PrimaryContactNumber { get; set; }
        public string EmergencyContactNumber { get; set; }
        public string Email { get; set; }
        public string Qualification { get; set; }
        public string Occupation { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string LegalFirstName { get; set; }
        public string LegalMiddleName { get; set; }
        public string LegalLastName { get; set; }
        public string LegalEmergencyCoNumber { get; set; }
        public string LegalEmail { get; set; }
        public string LegalRelationship { get; set; }
        public bool LegalStatus { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public int? StateId { get; set; }
        public int? CountryId { get; set; }
        public string ScreeningNumber { get; set; }
        public DateTime? DateOfScreening { get; set; }
        //public DateTime? DateOfScreening
        //{
        //    get => _DateOfScreening.UtcDate();
        //    set => _DateOfScreening = value.UtcDate();
        //}
        public string RandomizationNumber { get; set; }
        public DateTime? DateOfRandomization { get; set; }
        //public DateTime? DateOfRandomization
        //{
        //    get => _DateOfRandomization.UtcDate();
        //    set => _DateOfRandomization = value.UtcDate();
        //}

        public string Language { get; set; }
        public string ParentProjectCode { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public bool IsLocked { get; set; }
        public string PatientStatusName { get; set; }
        public ScreeningPatientStatus? PatientStatusId { get; set; }
        public bool IsShowEconsentIcon { get; set; }
        public bool IsEconsentReviewPending { get; set; }
        public List<EconsentReviewDetails> EconsentReviewDetails { get; set; }
        public bool IsmultipleEconsentReviewDetails { get; set; }
        public int? ZipCode { get; set; }
        public int? UserId { get; set; }
        public string Gen { get; set; }

    }

    public class RandomizationNumberDto
    {
        public string RandomizationNumber { get; set; }
        public string ScreeningNumber { get; set; }
        public bool? IsManualRandomNo { get; set; }
        public bool? IsSiteDependentRandomNo { get; set; }
        public int? RandomNoLength { get; set; }
        public bool? IsAlphaNumRandomNo { get; set; }
        public string? PrefixRandomNo { get; set; }
        public bool? IsManualScreeningNo { get; set; }
        public bool? IsSiteDependentScreeningNo { get; set; }
        public int? ScreeningLength { get; set; }
        public bool? IsAlphaNumScreeningNo { get; set; }
        public string? PrefixScreeningNo { get; set; }
        public int ScreeningNoseries { get; set; }
        public int RandomizationNoseries { get; set; }
        public int ProjectId { get; set; }
        public int ParentProjectId { get; set; }
        public bool IsTestSite { get; set; }

    }
}