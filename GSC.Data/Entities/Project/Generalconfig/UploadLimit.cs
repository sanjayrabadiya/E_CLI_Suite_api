using GSC.Common.Base;
using GSC.Common.Common;


namespace GSC.Data.Entities.Project.Generalconfig
{
    public class UploadLimit:BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int Uploadlimit { get; set;}
    }
}
