using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class TestGroupDto : BaseDto
    {
        [Required(ErrorMessage = "Test Group Name is required.")]
        public string TestGroupName { get; set; }

        public string Notes { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CompanyId { get; set; }
    }

    public class TestGroupGridDto : BaseAuditDto
    {
        public string TestGroupName { get; set; }
        public string Notes { get; set; }
    }
}