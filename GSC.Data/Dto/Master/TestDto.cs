using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.Master
{
    public class TestDto : BaseDto
    {
        [Required(ErrorMessage = "Test Name is required.")]
        public string TestName { get; set; }

        [Required(ErrorMessage = "Test Group is required.")]
        public int TestGroupId { get; set; }

        [Required(ErrorMessage = "Anticoagulant is required.")]
        public string Anticoagulant { get; set; }

        public string Notes { get; set; }

      //  public int? CompanyId { get; set; }

        public TestGroup TestGroup { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}