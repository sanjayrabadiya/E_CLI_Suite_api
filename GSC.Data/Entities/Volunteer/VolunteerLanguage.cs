using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Common.Common;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerLanguage : BaseEntity, ICommonAduit
    {
        public int VolunteerId { get; set; }

        public int LanguageId { get; set; }

        public bool IsRead { get; set; }

        public bool IsWrite { get; set; }

        public bool IsSpeak { get; set; }

        public string Note { get; set; }

        public Language Language { get; set; }
    }
}