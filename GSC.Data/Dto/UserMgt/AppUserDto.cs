using System;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.UserMgt
{
    public class AppUserDto
    {
        [Required] [Key] public Guid UserId { get; set; }

        [Required] [StringLength(255)] public string UserName { get; set; }
    }
}