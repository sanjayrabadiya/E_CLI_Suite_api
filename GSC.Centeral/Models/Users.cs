using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Centeral.Models
{
    [Table("Users")]
    public class Users: BaseEntity
    {

        private DateTime? _LastLoginDate;

        private DateTime? _ValidFrom;


        private DateTime? _ValidTo;
        public string UserName { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }      

        public string Email { get; set; }        

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
        public bool IsLogin { get; set; }
        public string ProfilePic { get; set; }     
        public int? CompanyID { get; set; }

        //private DateTime? _createdDate;
        //private DateTime? _deletedDate;

        //private DateTime? _modifiedDate;
        //public int Id { get; set; }

        //public DateTime? CreatedDate
        //{
        //    get => _createdDate?.UtcDateTime();
        //    set => _createdDate = value?.UtcDateTime();
        //}

        //public int? CreatedBy { get; set; }

        //public DateTime? ModifiedDate
        //{
        //    get => _modifiedDate.UtcDateTime();
        //    set => _modifiedDate = value?.UtcDateTime();
        //}

        //public int? ModifiedBy { get; set; }

        //public DateTime? DeletedDate
        //{
        //    get => _deletedDate?.UtcDateTime();
        //    set => _deletedDate = value?.UtcDateTime();
        //}

        //public int? DeletedBy { get; set; }
    }
}
