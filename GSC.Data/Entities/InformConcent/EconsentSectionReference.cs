using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentSectionReference : BaseEntity,ICommonAduit
    {
        public int EconsentSetupId { get; set; }
        public int SectionNo { get; set; }
        public string ReferenceTitle { get; set; }
        public string FilePath { get; set; }
        public EconsentSetup EconsentSetup { get; set; }
    }
}
