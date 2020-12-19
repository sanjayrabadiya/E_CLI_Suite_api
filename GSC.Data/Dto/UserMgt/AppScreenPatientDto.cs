using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.UserMgt
{
    public class AppScreenPatientDto : BaseDto
    {
        public string ScreenCode { get; set; }
        public string ScreenName { get; set; }
        public int SeqNo { get; set; }
    }
}
