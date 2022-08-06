using System;
using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerGridDto : BaseAuditDto
    {
        public string VolunteerNo { get; set; }

        public string RefNo { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string AliasName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? FromAge { get; set; }

        public int? ToAge { get; set; }

        public string Religion { get; set; }

        public string Occupation { get; set; }

        public string Education { get; set; }

        public decimal? AnnualIncome { get; set; }

        public string Gender { get; set; }

        public string Race { get; set; }

        public string MaritalStatus { get; set; }

        public string PopulationType { get; set; }

        public string ProfilePic { get; set; }

        public string Relationship { get; set; }
        public string Address { get; set; }
        public string FullName => FirstName + " " + MiddleName + " " + LastName;
        public string ProfilePicPath { get; set; }
        public string Foods { get; set; }
        public string FoodType { get; set; }
        public DateTime? RegisterDate { get; set; }
        public string StatusName { get; set; }
        public string Blocked { get; set; }
        public bool IsBlockAdd { get; set; }
        public bool IsBlockDisplay { get; set; }
        public bool IsScreeningHisotry { get; set; }
        public bool IsDeleteRole { get; set; }
        public bool IsScreening { get; set; }

        public string ContactNo { get; set; }
        public int? CompanyId { get; set; }
        public List<ScreeningHistory> ScreeningHistory { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BMI { get; set; }

    }

    public class VolunteerSearchDto
    {
        public int Id { get; set; }

        public string VolunteerNo { get; set; }

        public string FullName { get; set; }

        public string AliasName { get; set; }

        public int? FromAge { get; set; }

        public int? ToAge { get; set; }

        public Gender? GenderId { get; set; }

        public int? PopulationTypeId { get; set; }

        public string ContactNo { get; set; }

        public string CityName { get; set; }
        public string CityAreaName { get; set; }

        public VolunteerStatus? Status { get; set; }

        public bool? IsDeleted { get; set; }
        public string TextSearch { get; set; }
        public bool? IsBlocked { get; set; }
        public int LastScreening { get; set; }
        public int PeriodNo { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public DateTime? FromRegistration { get; set; }
        public DateTime? ToRegistration { get; set; }
        public int? StudyId { get; set; }
    }

    public class VolunteerAttendaceDto : BaseDto
    {
        public string VolunteerNo { get; set; }

        public string RefNo { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string AliasName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? FromAge { get; set; }

        public int? ToAge { get; set; }
        public string Gender { get; set; }
        public bool Blocked { get; set; }
        public string Race { get; set; }
        public int? ProjectSubjectId { get; set; }
        public int? ScreeningEntryId { get; set; }
        public string FullName { get; set; }
    }
}