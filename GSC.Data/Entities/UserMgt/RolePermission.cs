using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class RolePermission : BaseEntity, ICommonAduit
    {
        public int UserRoleId { get; set; }

        public int AppScreenId { get; set; }

        public string ScreenCode { get; set; }

        public bool IsView { get; set; }

        public bool IsAdd { get; set; }

        public bool IsEdit { get; set; }

        public bool IsDelete { get; set; }

        public bool IsExport { get; set; }
        public bool IsSync { get; set; }

        public AppScreen AppScreens { get; set; }
    }
}