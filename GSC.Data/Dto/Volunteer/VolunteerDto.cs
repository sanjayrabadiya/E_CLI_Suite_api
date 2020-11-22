using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Volunteer;
using GSC.Helper;
using GSC.Shared.DocumentService;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerDto : BaseDto
    {
        public string VolunteerNo { get; set; } = "Auto Number";

        public string RefNo { get; set; }

        [Required(ErrorMessage = "Volunteer Last Name is required.")]
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string FullName => FirstName + " " + MiddleName + " " + LastName;

        public string AliasName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? FromAge { get; set; }

        public int? ToAge { get; set; }

        public int? ReligionId { get; set; }

        public int? OccupationId { get; set; }

        public string Education { get; set; }

        public decimal? AnnualIncome { get; set; }

        public Gender? GenderId { get; set; }

        public int? RaceId { get; set; }

        public int? MaritalStatusId { get; set; }

        public int? PopulationTypeId { get; set; }

        public string Relationship { get; set; }

        public int? CompanyId { get; set; }

        public DateTime? RegisterDate { get; set; }

        public string ProfilePic { get; set; }

        public string ProfilePicPath { get; set; }

        public IList<VolunteerAddress> Addresses { get; set; } = null;

        public IList<VolunteerContact> Contacts { get; set; } = null;

        public IList<VolunteerFood> Foods { get; set; } = null;

        public IList<VolunteerLanguage> Languages { get; set; } = null;

        public FileModel FileModel { get; set; }

        public VolunteerStatus Status { get; set; }

        public string StatusName { get; set; }

        public List<AuditTrail> Changes { get; set; }
        public bool? IsBlocked { get; set; }
        public bool IsBlockDisplay { get; set; }
        public bool IsScreening { get; set; }
    }

    public class VolunteerStatusCheck
    {
        public int Id { get; set; }
        public string VolunteerNo { get; set; }
        public bool IsNew { get; set; }
        public VolunteerStatus Status { get; set; }

        public string StatusName { get; set; }
    }
}