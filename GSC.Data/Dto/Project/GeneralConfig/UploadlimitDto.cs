using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.Project.GeneralConfig
{
    public class UploadlimitDto: BaseDto
    {
        [Required(ErrorMessage = "Project Id is required.")]
        public int? ProjectId { get; set; }
        [Required(ErrorMessage = "Upload limit is required.")]
        public int? Uploadlimit { get; set; }

    }
}
