using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.Project.EditCheck
{
    public class EditCheck : BaseEntity
    {
        public int ProjectDesignId { get; set; }
        public string AutoNumber { get; set; }
        public string CheckFormula { get; set; }
        public string TargetFormula { get; set; }
        public string SourceFormula { get; set; }
        public int? CompanyId { get; set; }
        public ProjectDesign ProjectDesign { get; set; }
        public IEnumerable<EditCheckDetail> EditCheckDetails { get; set; }
        public bool IsFormula { get; set; }
        public bool IsReferenceVerify { get; set; }
        public bool IsOnlyTarget { get; set; }
        public string SampleResult { get; set; }
        public string ErrorMessage { get; set; }

    }
}