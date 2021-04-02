using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.UserMgt
{
    public class UserSettingDto : BaseDto
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int? StudyId { get; set; }
        public int? CountryId { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
    }
}
