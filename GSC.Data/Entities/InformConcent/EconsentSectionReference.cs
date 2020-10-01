using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentSectionReference : BaseEntity
    {
        //public int Id { get; set; }
        public int EconsentDocId { get; set; }
        public int SectionNo { get; set; }
        public string ReferenceTitle { get; set; }
        public string FilePath { get; set; }
    }
}
