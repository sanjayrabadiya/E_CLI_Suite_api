using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
  public  class ResourceTypeDto: BaseDto
    {
        [Required(ErrorMessage = "Resource Name is required.")]
        public string ResourceName { get; set; }
        [Required(ErrorMessage = "Resource Code is required.")]
        public string ResourceCode { get; set; }
    }

    public class ResourceTypeGridDto: BaseAuditDto
    {
        public string ResourceName { get; set; }
        public string ResourceCode { get; set; }
    }
}
