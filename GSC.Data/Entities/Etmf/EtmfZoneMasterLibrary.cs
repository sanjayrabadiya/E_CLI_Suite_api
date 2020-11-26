using GSC.Common.Base;
using System;
using System.Collections.Generic;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
    public class EtmfZoneMasterLibrary : BaseEntity, ICommonAduit
    {
        public string ZoneNo { get; set; }
        public string ZonName { get; set; }
        public string Version { get; set; }
        public string FileName { get; set; }
        public ICollection<EtmfSectionMasterLibrary> EtmfSectionMasterLibrary { get; set; }
    }
}
