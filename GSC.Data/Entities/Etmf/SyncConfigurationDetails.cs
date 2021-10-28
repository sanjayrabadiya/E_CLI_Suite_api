using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
   public class SyncConfigurationDetails : BaseEntity, ICommonAduit
    {
        public int SyncConfigurationId { get; set; }
        public int ProjectWorkplaceDetailId { get; set; }
        public int ProjectWorkPlaceZoneId { get; set; }
        public int ProjectWorkplaceSectionId { get; set; }
        public int ProjectWorkplaceSubSectionId { get; set; }
        public int ProjectWorkplaceSubSectionArtifactId { get; set; }
        public int ProjectWorkplaceArtificateId { get; set; }
    }
}
