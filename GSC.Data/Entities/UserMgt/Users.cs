using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared;

namespace GSC.Data.Entities.UserMgt
{
    [Table("Users")]
    public class User : BaseEntity
    {
        private DateTime? _DateOfBirth;


        private DateTime? _LastLoginDate;

        private DateTime? _ValidFrom;


        private DateTime? _ValidTo;
        public string UserName { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public Gender GenderId { get; set; }

        public string Email { get; set; }

        public DateTime? DateOfBirth
        {
            get => _DateOfBirth?.UtcDate();
            set => _DateOfBirth = value?.UtcDate();
        }

        public string Phone { get; set; }

        public DateTime? ValidFrom
        {
            get => _ValidFrom?.UtcDate();
            set => _ValidFrom = value?.UtcDate();
        }

        public DateTime? ValidTo
        {
            get => _ValidTo?.UtcDate();
            set => _ValidTo = value?.UtcDate();
        }

        public int FailedLoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLoginDate
        {
            get => _LastLoginDate?.UtcDate();
            set => _LastLoginDate = value?.UtcDate();
        }

        public string LastIpAddress { get; set; }


        public string LastSystemName { get; set; }

        public int? CompanyId { get; set; }

        public bool IsFirstTime { get; set; }

        public bool IsLogin { get; set; }

        public List<UserRole> UserRoles { get; set; } = null;

        public string ProfilePic { get; set; }

        public string RoleTokenId { get; set; }

        public bool IsPowerAdmin { get; set; }

        public PrefLanguage? Language { get; set; }
        public string? SignaturePath { get; set; }

    }
}