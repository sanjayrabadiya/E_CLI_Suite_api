﻿using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerImageDto : BaseDto
    {
        public int VolunteerId { get; set; }
        public byte[] BiometricBinary { get; set; }
        public byte[] ThumbData { get; set; }
        public string FilePath { get; set; }
        public string MimeType { get; set; }

        [Required(ErrorMessage = "File Name is required.")]
        public string FileName { get; set; }
    }
}