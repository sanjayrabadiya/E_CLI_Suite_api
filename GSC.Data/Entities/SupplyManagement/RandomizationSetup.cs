
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.SupplyManagement
{
    public class RandomizationSetup : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public bool StudyLevel { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public Entities.Master.Project Project { get; set; }
    }
}
