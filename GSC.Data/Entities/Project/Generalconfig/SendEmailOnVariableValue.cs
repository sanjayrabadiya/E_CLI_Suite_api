using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.Project.Generalconfig
{
    public class SendEmailOnVariableValue : BaseEntity, ICommonAduit
    {
        public int SendEmailOnVariableChangeSettingId { get; set; }
        public int ProjectDesignVariableValueId { get; set; }

        public SendEmailOnVariableChangeSetting SendEmailOnVariableChangeSetting { get; set; }
        public ProjectDesignVariableValue ProjectDesignVariableValue { get; set; }
    }
}
