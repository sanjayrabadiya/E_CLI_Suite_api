using GSC.Common.Base;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerBiometric : BaseEntity
    {
        public int VolunteerId { get; set; }

        public byte[] BiometricBinary { get; set; }

        public short Type { get; set; }
    }
}