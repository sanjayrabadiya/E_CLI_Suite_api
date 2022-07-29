namespace GSC.Data.Dto.UserMgt
{
    public class RolePermissionDto
    {
        public int UserRoleId { get; set; }

        public int AppScreenId { get; set; }

        public string ScreenCode { get; set; }

        public string ScreenName { get; set; }

        public int? ParentAppScreenId { get; set; }
        public int? RolePermissionId { get; set; }

        public bool IsView { get; set; }

        public bool IsAdd { get; set; }

        public bool IsEdit { get; set; }

        public bool IsDelete { get; set; }

        public bool IsExport { get; set; }
        public bool IsSync { get; set; }

        public bool IsAll { get; set; }

        public bool CanView { get; set; }

        public bool CanAdd { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public bool CanExport { get; set; }
        public bool CanSync { get; set; }

        public bool CanAll { get; set; }
        public bool hasChild { get; set; }
    }
    public class SidebarMenuRolePermissionDto
    {
        public int UserRoleId { get; set; }

        public int AppScreenId { get; set; }

        public string ScreenCode { get; set; }

        public string ScreenName { get; set; }

        public int? ParentAppScreenId { get; set; }
        public int? RolePermissionId { get; set; }

        public bool hasChild { get; set; }
    }
}