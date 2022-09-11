using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.Project.GeneralConfig
{
    public class ProjectSettingsDto : BaseDto
    {
        public int ProjectId { get; set; }
        public bool IsCtms { get; set; }
        public bool IsEicf { get; set; }
    }
}
