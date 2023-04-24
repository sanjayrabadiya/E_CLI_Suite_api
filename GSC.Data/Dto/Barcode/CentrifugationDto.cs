
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Barcode
{
    public class CentrifugationDto : BaseDto
    {
        public int ProjectId { get; set; }
        public string MachineName { get; set; }
        public int RCMRPM { get; set; }
        public int Min { get; set; }
        public int Temprature { get; set; }
    }

    public class CentrifugationGridDto : BaseAuditDto
    {
        public string MachineName { get; set; }
        public int RCMRPM { get; set; }
        public int Min { get; set; }
        public int Temprature { get; set; }
    }
}
