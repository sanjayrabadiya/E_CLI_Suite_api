using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerLanguageDto : BaseDto
    {
        public int VolunteerId { get; set; }

        [Required(ErrorMessage = "Language is required.")]
        public int LanguageId { get; set; }

        public bool IsRead { get; set; }

        public bool IsWrite { get; set; }

        public bool IsSpeak { get; set; }

        public string Note { get; set; }

        public string LanguageName { get; set; }
        public List<AuditTrail> Changes { get; set; }
    }
}