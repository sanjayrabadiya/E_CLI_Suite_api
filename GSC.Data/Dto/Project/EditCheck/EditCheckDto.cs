using System.Collections.Generic;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.EditCheck
{
    public class EditCheckDto : BaseAuditDto
    {
        public int ProjectDesignId { get; set; }
        public string AutoNumber { get; set; }
        public string CheckFormula { get; set; }
        public string TargetFormula { get; set; }
        public string SourceFormula { get; set; }
        public string StatusName { get; set; }
        public int? CompanyId { get; set; }
        public List<EditCheckDetailDto> EditCheckDetails { get; set; }
        public bool IsFormula { get; set; }
        public bool IsReferenceVerify { get; set; }
        public bool IsOnlyTarget { get; set; }
        public string SampleResult { get; set; }
        public string ErrorMessage { get; set; }
    }
}