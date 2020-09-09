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

        public TestGroup TestGroup { get; set; }
        public int? CompanyId { get; set; }
    }

    public class TestGridDto : BaseAuditDto
    {
        public string TestName { get; set; }
        public int TestGroupId { get; set; }
        public string Anticoagulant { get; set; }
        public string Notes { get; set; }
        public TestGroup TestGroup { get; set; }
    }
}