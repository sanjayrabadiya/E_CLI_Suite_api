using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Configuration
{
    public class ReportSetting : BaseEntity, ICommonAduit
    {
        public bool IsClientLogo { get; set; }
        public bool IsCompanyLogo { get; set; }
        public bool IsInitial { get; set; }
        public bool IsSponsorNumber { get; set; }
        public bool IsScreenNumber { get; set; }
        public bool IsSubjectNumber { get; set; }
        public decimal LeftMargin { get; set; }
        public decimal RightMargin { get; set; }
        public decimal TopMargin { get; set; }
        public decimal BottomMargin { get; set; }
        public int? CompanyId { get; set; }
    }
}