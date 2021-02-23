using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Configuration
{
   public class ReleaseSetting
    {
        public int Id { get; set; }
        public string VersionNumber { get; set; }

        public DateTime ReleaseDate { get; set; }
        public string ReleaseBy { get; set; }
     }
}
