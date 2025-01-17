﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EconcentChatParameterDto
    {
        [Required(ErrorMessage = "User Id is required.")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Page Size is required.")]
        public int PageSize { get; set; }
        public int PageNumber { get; set; }       
    }
}
