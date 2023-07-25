using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.CTMS
{
    public class LettersActivity : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int ActivityId { get; set; }
        public int LettersFormateId { get; set; }
        public int CtmsMonitoringId { get; set; }
        public string Email { get; set; }
        public int? UserIntigration { get; set; }
        public string FilePath { get; set; }
        public string AttachmentPath { get; set; }  
        public string LetterBody { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
        public CtmsActivity Activity { get; set; }
        public LettersFormate LettersFormate { get; set; }
        public CtmsMonitoring CtmsMonitoring { get; set; }
    }
}
