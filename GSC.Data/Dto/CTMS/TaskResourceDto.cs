using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.CTMS
{
    public class TaskResourceDto : BaseDto
    {
        public int TaskMasterId { get; set; }
        public int ResourceTypeId { get; set; } 
    }
    public class TaskResourceGridDto : BaseAuditDto
    {
        public string ResourceType { get; set; }
        public string ResourceSubType { get; set; }
    }

}
