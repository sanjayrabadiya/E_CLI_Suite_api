using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class ActivityDto : BaseDto
    {
        [Required(ErrorMessage = "Activity Name is required.")]
        public string ActivityName { get; set; }

        public int? CompanyId { get; set; }
    }
}