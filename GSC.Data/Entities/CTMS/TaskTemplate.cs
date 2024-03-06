using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.CTMS
{
    public class TaskTemplate : BaseEntity, ICommonAduit
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
    }
}
