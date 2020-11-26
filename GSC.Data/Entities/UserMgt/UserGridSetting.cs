using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class UserGridSetting : BaseEntity, ICommonAduit
    {
        public int UserId { get; set; }
        public string ScreenCode { get; set; }
        public string ColumnField { get; set; }
        public string ColumnTitle { get; set; }
        public int? ColumnWidth { get; set; }
        public bool IsColumnVisible { get; set; }
    }
}