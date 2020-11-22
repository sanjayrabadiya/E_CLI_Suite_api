using GSC.Common.Base;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateValueSchedule : BaseEntity
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int ScreeningEntryId { get; set; }
        public bool IsClosed { get; set; }
        public bool IsStarted { get; set; }
        public string Message { get; set; }
        public bool IsVerify { get; set; }
    }
}