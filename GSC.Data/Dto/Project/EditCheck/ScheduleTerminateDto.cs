using GSC.Helper;

namespace GSC.Data.Dto.Project.EditCheck
{
    public class ScheduleTerminateDto
    {
        public CollectionSources? CollectionSource { get; set; }
        public DataType? DataType { get; set; }
        public string Value { get; set; }
        public Operator Operator { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int TargetProjectDesignTemplateId { get; set; }
        public int TargetProjectDesignVariableId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int TargetProjectDesignVisitId { get; set; }
        public int ProjectScheduleId { get; set; }
    }
}
