using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Configuration
{
    public class UploadSetting : BaseEntity, ICommonAduit
    {
        public string ImagePath { get; set; }

        public string DocumentPath { get; set; }
        public string ImageUrl { get; set; }
        public string DocumentUrl { get; set; }

        public int CompanyId { get; set; }
        public int? DataRecycleDays { get; set; }
        public UploadLimitType? UploadLimitType { get; set; }
    }
}