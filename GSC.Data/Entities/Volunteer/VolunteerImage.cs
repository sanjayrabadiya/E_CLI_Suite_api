using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerImage : BaseEntity
    {
        public int VolunteerId { get; set; }
        public byte[] BiometricBinary { get; set; }
        public byte[] ThumbData { get; set; }
        public string FilePath { get; set; }
        public string MimeType { get; set; }
        public string FileName { get; set; }
    }
}