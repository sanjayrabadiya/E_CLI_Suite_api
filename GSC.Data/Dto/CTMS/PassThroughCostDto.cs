
using GSC.Data.Entities.Common;
using GSC.Helper;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
    public class PassThroughCostDto : BaseDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Activity is required.")]
        public int PassThroughCostActivityId { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public int CountryId { get; set; }

        [Required(ErrorMessage = "Flg Type is required.")]
        public string BudgetFlgType { get; set; }
        public int? UnitId { get; set; }
        public int? NoOfUnit { get; set; }
        public int? NoOfPatient { get; set; }

        [Required(ErrorMessage = "Rate is required.")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Frequency is required.")]
        public int Frequency { get; set; }
        public decimal? Total { get; set; }
        public decimal? CurrenRate { get; set; }
    }
    public class PassThroughCostGridDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int PassThroughCostActivityId { get; set; }
        public string PassThroughCostActivityName { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyRate { get; set; }
        public int GlobleCurrencyId { get; set; }
        public string GlobleCurrencySymbol { get; set; }
        public string LocalCurrencySymbol { get; set; }
        public BudgetFlgType BudgetFlgType { get; set; }
        public string BudgetFlgTypeName { get; set; }
        public int? UnitId { get; set; }
        public string UnitType { get; set; }
        public int? NoOfUnit { get; set; }
        public int? NoOfPatient { get; set; }
        public string Rate { get; set; }
        public int Frequency { get; set; }
        public decimal? Total { get; set; }

    }
    public class DropDownPassThroughCostDto
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string CurrencyType { get; set; }
        public string CurrencySymbol { get; set; }
        public int LocCurrencyId { get; set; }
    }
}
