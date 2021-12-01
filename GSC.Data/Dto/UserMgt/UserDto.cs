using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;
using GSC.Shared.Generic;

namespace GSC.Data.Dto.UserMgt
{
    public class UserDto
    {
        public UserDto()
        {
            UserRoles = new List<UserRole>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "User Name is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        //[Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        public DateTime? _DateOfBirth { get; set; }
        public DateTime? DateOfBirth
        {
            get => _DateOfBirth?.UtcDate();
            set => _DateOfBirth = value?.UtcDate();
        }

        public string Role { get; set; }

        public string Phone { get; set; }

        public int? CompanyId { get; set; }
        public DateTime? ValidFrom { get; set; }
        //public DateTime? ValidFrom
        //{
        //    get => _ValidFrom?.UtcDate();
        //    set => _ValidFrom = value?.UtcDate();
        //}
        public DateTime? ValidTo { get; set; }

        //public DateTime? ValidTo
        //{
        //    get => _ValidTo?.UtcDate();
        //    set => _ValidTo = value?.UtcDate();
        //}

        public int? FailedLoginAttempts { get; set; }

        public bool IsLocked { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public string LastIpAddress { get; set; }

        public string LastSystemName { get; set; }

        public List<UserRole> UserRoles { get; set; }

        public string ProfilePic { get; set; }

        public string ProfilePicPath { get; set; }

        public FileModel FileModel { get; set; }

        public string RoleTokenId { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsPowerAdmin { get; set; }
        public bool IsFirstTime { get; set; }
        public int? Language { get; set; }
        public string LanguageShortName { get; set; }
        //public string? SignaturePath { get; set; }
        public string CompanyName { get; set; }

        public UserMasterUserType? UserType { get; set; }
    }

    public class UserGridDto : BaseAuditDto
    {
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public string ProfilePic { get; set; }
        public bool IsLocked { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string ProfilePicPath { get; set; }
        public string ScreeningNumber { get; set; }
        public DateTime? DateOfScreening { get; set; }
        public string RandomizationNumber { get; set; }
        public DateTime? DateOfRandomization { get; set; }
    }
}