﻿using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateReview : BaseEntity
    {
        public int ScreeningTemplateId { get; set; }
        public int RoleId { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public short ReviewLevel { get; set; }
        public bool IsRepeat { get; set; }


        [ForeignKey("RoleId")] public SecurityRole Role { get; set; }
    }
}