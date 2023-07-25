using GSC.Data.Entities.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
    public class LettersActivityDto : BaseDto
    {
        [Required (ErrorMessage = "Project is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Activity Type is Required")]
        public int ActivityId { get; set; }

        [Required(ErrorMessage = "Letters Formate Type is Required")]
        public int LettersFormateId { get; set; }

        [Required(ErrorMessage = "Ctms Monitoring is Required")]
        public int CtmsMonitoringId { get; set; }
        public string Email { get; set; }
        public int? UserIntigration { get; set; }  
        public string FilePath { get; set; }
        public string AttachmentPath { get; set; }
        public string LetterBody { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public string Project { get; set; }
        public User User { get; set; }

    }
    public class LettersActivityGridDto : BaseAuditDto
    {
        public string Project { get; set; }
        public string Activity { get; set; }
        public string LettersFormate { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
        public string Email { get; set; }
        public string UserIntigration { get; set; }
        public string FilePath { get; set; }
        
    }
    public class LettersActivityDateDropDown : BaseDto
    {
        public DateTime? Value { get; set; }
        public string Code { get; set; }
        public int ActivityType { get; set; }    
    }
    public class SendMailModel
    {
        public int Id { get; set; } 
        public string Email { get; set; }
        public List<UserModel> UserModel { get; set; }
    }
    public class UserModel
    {
        public int userId { get; set; }
    }
}
