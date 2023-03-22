using System;
using GSC.Common.Base;
using GSC.Helper;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.Master;
using GSC.Common.Common;
using GSC.Shared.Extension;
using System.Collections.Generic;
using GSC.Data.Entities.InformConcent;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Attendance
{
    public class Randomization : BaseEntity, ICommonAduit
    {


        public ScreeningPatientStatus PatientStatusId { get; set; }
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
        public int? CountryId { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }
        public int? ZipCode { get; set; }
        public string LegalFirstName { get; set; }
        public string LegalMiddleName { get; set; }
        public string LegalLastName { get; set; }
        public string LegalEmergencyCoNumber { get; set; }
        public string LegalEmail { get; set; }
        public Relationship? LegalRelationship { get; set; }
        public bool LegalStatus { get; set; }
        public Country Country { get; set; }
        public State State { get; set; }
        public City City { get; set; }
        public Language Language { get; set; }
        public string ScreeningNumber { get; set; }
        public DateTime? DateOfScreening { get; set; }
        public string RandomizationNumber { get; set; }

        public DateTime? DateOfRandomization { get; set; }

        public int ProjectId { get; set; }
        public int? CompanyId { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
        public virtual ScreeningEntry ScreeningEntry { get; set; }
        public string WithdrawSignaturePath { get; set; }
        public int? UserId { get; set; }
        public List<EconsentReviewDetails> EconsentReviewDetails { get; set; }
        public User User { get; set; }
        public double? StudyVersion { get; set; }

        public int? LARUserId { get; set; }

        public Gender? Genderfactor { get; set; }
        public DaitoryFector? Diatoryfactor { get; set; }

        public Jointfactor? Jointfactor { get; set; }

        public Eligibilityfactor? Eligibilityfactor { get; set; }

        public string Agefactor { get; set; }
        public string BMIfactor { get; set; }
        public string ProductCode { get; set; }
    }
}