using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Dto.Project.Generalconfig
{
    public class SendEmailOnVariableValueDto : BaseEntity, ICommonAduit
    {
        public int SendEmailOnVariableChangeSettingId { get; set; }
        public int ProjectDesignVariableValueId { get; set; }
    }
}
