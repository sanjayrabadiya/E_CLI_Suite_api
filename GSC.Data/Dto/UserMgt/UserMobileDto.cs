using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.UserMgt
{
    public class UserMobileDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User Name is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? CompanyId { get; set; }

        public int FailedLoginAttempts { get; set; }

        public string IsLocked { get; set; }

        public string IsActive { get; set; }

        public string IsDeleted { get; set; }

        public bool IsFirstTime { get; set; }
        public PrefLanguage? Language { get; set; }

        public string LanguageShortName { get; set; }
    }
}
