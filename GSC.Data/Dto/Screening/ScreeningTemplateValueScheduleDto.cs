namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateValueScheduleDto
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int ScreeningEntryId { get; set; }
        public string Message { get; set; }
        public bool IsClosed { get; set; }
        public bool IsStarted { get; set; }
        public bool IsVerify { get; set; }
    }
}