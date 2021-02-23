using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Configuration
{
   public class ReleaseSettingDto
    {
        public int Id { get; set; }
        public string VersionNumber { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ReleaseBy { get; set; }
        public object Value { get; set; }
    }
}
