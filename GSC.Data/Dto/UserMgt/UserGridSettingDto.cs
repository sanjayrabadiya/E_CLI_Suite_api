using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.UserMgt
{
    public class UserGridSettingDto : BaseDto
    {
        [Required(ErrorMessage = "Screen Code is required.")]
        public string ScreenCode { get; set; }

        [Required(ErrorMessage = "Column Field is required.")]
        public string ColumnField { get; set; }

        [Required(ErrorMessage = "Column Title is required.")]
        public string ColumnTitle { get; set; }

        public int? ColumnWidth { get; set; }
        public bool IsColumnVisible { get; set; }
    }
}