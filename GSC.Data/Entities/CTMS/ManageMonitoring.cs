using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.CTMS
{
    public class ManageMonitoring : BaseEntity, ICommonAduit
    {
        public int ActivityId { get; set; }
        public int ProjectId { get; set; }
        public int VariableTemplateId { get; set; }
        public int? CompanyId { get; set; }

        public Activity Activity { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
        public VariableTemplate VariableTemplate { get; set; }
    }
}