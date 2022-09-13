using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentGlossary : BaseEntity,ICommonAduit
    {
        public int EconsentSetupId { get; set; }
        public string Word { get; set; }
        public string WordMeaning { get; set; }
        public EconsentSetup EconsentSetup { get; set; }
    }
}
