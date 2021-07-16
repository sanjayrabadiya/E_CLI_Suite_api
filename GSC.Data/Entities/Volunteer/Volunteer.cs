using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Entities.Volunteer
{
    public class Volunteer : BaseEntity
    {
        public DateTime? DateOfBirth { get; set; }
        public DateTime? RegisterDate { get; set; }
        public string VolunteerNo { get; set; }

        public string RefNo { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string AliasName { get; set; }

        public int? FromAge { get; set; }

        public int? ToAge { get; set; }

        public int? ReligionId { get; set; }
        public Religion Religion { get; set; }

        public int? OccupationId { get; set; }
        public Occupation Occupation { get; set; }

        public string Education { get; set; }

        public decimal? AnnualIncome { get; set; }

        public Gender? GenderId { get; set; }
        public int? RaceId { get; set; }
        public Race Race { get; set; }

        public int? MaritalStatusId { get; set; }
        public MaritalStatus MaritalStatus { get; set; }

        public int? PopulationTypeId { get; set; }
        public PopulationType PopulationType { get; set; }

        public int? FoodTypeId { get; set; }

        public FoodType FoodType { get; set; }

        public string Relationship { get; set; }


        public int? CompanyId { get; set; }

        public string ProfilePic { get; set; }

        public IList<VolunteerAddress> Addresses { get; set; } = null;
        //public IList<VolunteerContact> Contacts { get; set; } = null;
        //public IList<VolunteerFood> Foods { get; set; } = null;
        //public IList<VolunteerLanguage> Languages { get; set; } = null;
        //public IList<VolunteerDocument> Documents { get; set; } = null;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FullName { get; set; }
        //public string FullName => FirstName + " " + MiddleName + " " + LastName;
        public bool? IsBlocked { get; set; }
        public bool IsScreening { get; set; }
        public VolunteerStatus Status { get; set; }
        public ICollection<Attendance.Attendance> Attendances { get; set; }
    }
}