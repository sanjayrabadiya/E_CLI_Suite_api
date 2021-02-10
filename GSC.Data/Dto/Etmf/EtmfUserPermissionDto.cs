using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Dto.Etmf
{
    public class EtmfUserPermissionDto
    {
        public int UserId { get; set; }

        public int ProjectWorkplaceDetailId { get; set; }
        public int ProjectWorkplaceId { get; set; }
        public int WorkPlaceFolderId { get; set; }
        public string WorkPlaceFolder { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }

        public int? ParentWorksplaceFolderId { get; set; }
        public int? EtmfUserPermissionId { get; set; }

        public bool IsView { get; set; }

        public bool IsAdd { get; set; }

        public bool IsEdit { get; set; }

        public bool IsDelete { get; set; }

        public bool IsExport { get; set; }

        public bool IsAll { get; set; }

        //public bool CanView { get; set; }

        //public bool CanAdd { get; set; }

        //public bool CanEdit { get; set; }

        //public bool CanDelete { get; set; }

        //public bool CanExport { get; set; }

        //public bool CanAll { get; set; }
        public bool hasChild { get; set; }
        //public List<EtmfUserPermissionDto> child { get; set; }
    }

    public class EtmfWorksplaceDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public List<EtmfUserPermissionDto> LstUserPermission { get; set; }
    }

}