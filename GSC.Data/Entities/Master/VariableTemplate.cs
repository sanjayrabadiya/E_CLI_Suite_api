using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Helper;

namespace GSC.Data.Entities.Master
{
    public class VariableTemplate : BaseEntity
    {
        public string TemplateCode { get; set; }
        public string ActivityName { get; set; }
        public string TemplateName { get; set; }
        public int DomainId { get; set; }
        public bool IsRepeated { get; set; }
        public int? CompanyId { get; set; }
        public List<VariableTemplateDetail> VariableTemplateDetails { get; set; }
        public Domain Domain { get; set; }
        public ActivityMode ActivityMode { get; set; }
        public IList<VariableTemplateNote> Notes { get; set; }
    }
}