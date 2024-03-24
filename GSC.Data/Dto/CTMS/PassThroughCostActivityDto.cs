using GSC.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
  public  class PassThroughCostActivityDto : BaseDto
    {
        [Required(ErrorMessage = "Name is Required.")]
        public string ActivityName {  get; set; }
    }

    public class PassThroughCostActivityGridDto : BaseAuditDto
    {
        public string ActivityName { get; set; }
    }
}
