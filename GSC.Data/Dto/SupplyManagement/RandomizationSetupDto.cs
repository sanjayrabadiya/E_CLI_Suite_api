using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.SupplyManagement
{
    public class RandomizationSetupDto : BaseDto
    {
        public int ProjectId { get; set; }
        public bool StudyLevel { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public Entities.Master.Project Project { get; set; }
    }

    public class RandomizationSetupGridDto : BaseAuditDto
    {
        public string StudyName { get; set; }
        public bool StudyLevel { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
    }
}
