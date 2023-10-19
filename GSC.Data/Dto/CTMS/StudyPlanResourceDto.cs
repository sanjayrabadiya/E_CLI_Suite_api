using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.CTMS
{

    public class StudyPlanResourceDto : BaseDto
    {
        public int StudyPlanTaskId { get; set; }
        public int ResourceTypeId { get; set; }
    }
    public class StudyPlanResourceGridDto : BaseAuditDto
    {
        public string ResourceType { get; set; }
        public string ResourceSubType { get; set; }
    }
}
