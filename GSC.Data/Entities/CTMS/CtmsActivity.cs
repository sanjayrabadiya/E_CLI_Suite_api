using GSC.Common.Base;
using GSC.Common.Common;
namespace GSC.Data.Entities.CTMS
{
    public class CtmsActivity : BaseEntity, ICommonAduit
    {
        public string ActivityCode { get; set; }
        public string ActivityName { get; set; }
    }
}
