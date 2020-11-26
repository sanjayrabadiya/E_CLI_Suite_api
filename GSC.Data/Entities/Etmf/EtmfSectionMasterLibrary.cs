using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class EtmfSectionMasterLibrary : BaseEntity, ICommonAduit
    {
        public int EtmfZoneMasterLibraryId { get; set; }
        public string Sectionno { get; set; }
        public string SectionName { get; set; }
        public EtmfZoneMasterLibrary EtmfZoneMasterLibrary { get; set; }
        public ICollection<EtmfArtificateMasterLbrary> EtmfArtificateMasterLbrary { get; set; }
    }
}
