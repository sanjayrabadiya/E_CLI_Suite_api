using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Master
{
    public class ScreeningNumberSettingsDto : BaseDto
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public bool IsManualScreeningNo { get; set; }
        public bool IsSiteDependentScreeningNo { get; set; }
        public int ScreeningLength { get; set; }
        public bool? IsAlphaNumScreeningNo { get; set; }
        public int ScreeningNoseries { get; set; }
        public int? ScreeningNoStartsWith { get; set; }
        public string PrefixScreeningNo { get; set; }
        public bool DisableRow { get; set; }
        public List<ScreeningNumberSettingsDto> ScreeningNumberSettingsSites { get; set; }
    }
}
