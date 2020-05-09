using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerBiometricDto : BaseDto
    {
        public int VolunteerId { get; set; }

        [Required(ErrorMessage = "Biometric Binary is required.")]
        public byte[] BiometricBinary { get; set; }

        public short Type { get; set; }
    }
}