using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;

namespace GSC.Data.Entities.UserMgt
{
    public class AppScreen : BaseEntity
    {
        public string ScreenCode { get; set; }

        public string ScreenName { get; set; }

        public int? ParentAppScreenId { get; set; }

        public bool IsMenu { get; set; }

        public bool IsPermission { get; set; }

        public bool IsView { get; set; }

        public bool IsAdd { get; set; }

        public bool IsEdit { get; set; }

        public bool IsDelete { get; set; }

        public bool IsExport { get; set; }

        public string UrlName { get; set; }

        public int SeqNo { get; set; }
        public string IconPath { get; set; }

        [NotMapped] public bool IsFavorited { get; set; }

        public bool? IsMaster { get; set; }
        public string TableName { get; set; }
    }
}