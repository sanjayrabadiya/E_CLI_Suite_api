using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.Master
{
    public class VariableTemplate : BaseEntity, ICommonAduit
    {
        public string TemplateCode { get; set; }
        //public int ActivityId { get; set; }
        public string TemplateName { get; set; }
        public int DomainId { get; set; }
        public bool IsRepeated { get; set; }
        public int? CompanyId { get; set; }
      //  public bool SystemType { get; set; }
        public List<VariableTemplateDetail> VariableTemplateDetails { get; set; }
        public Domain Domain { get; set; }
        public ActivityMode ActivityMode { get; set; }
        public IList<VariableTemplateNote> Notes { get; set; }
        //public AuditModule? ModuleId { get; set; }
        public int? AppScreenId { get; set; }
        //public Activity Activity { get; set; }
        public AppScreen AppScreen { get; set; }
    }
}