﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Helper.DocumentService;

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

        //[Required(ErrorMessage = "Gender is required.")]
        //public Gender GenderId { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Role { get; set; }

        public Entities.Location.Location Location { get; set; } = null;

        public int? ScopeNameId { get; set; }

        public string Phone { get; set; }

       
        public int? CompanyId { get; set; }

        
        public int? DepartmentId { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

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
        public string Note { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
}