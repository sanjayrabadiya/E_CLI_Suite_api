using GSC.Common.Base;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementEmailScheduleLog : BaseEntity
    {
        public int? ProjectId { get; set; }
        public int? RecordId { get; set; }
        public string RecurrenceType { get; set; }

        public string TriggerType { get; set; }

        public string Message { get; set; }

    }
}
