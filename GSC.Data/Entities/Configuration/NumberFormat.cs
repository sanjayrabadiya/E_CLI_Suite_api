using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Configuration
{
    public class NumberFormat : BaseEntity
    {
        public string KeyName { get; set; }
        public string PrefixFormat { get; set; }
        public string YearFormat { get; set; }
        public string MonthFormat { get; set; }
        public int StartNumber { get; set; }
        public int CompanyId { get; set; }
        public string SeparateSign { get; set; }
        public bool ResetYear { get; set; }
        public string Hint { get; set; }
        public int NumberLength { get; set; }
        public bool IsManual { get; set; }
    }
}