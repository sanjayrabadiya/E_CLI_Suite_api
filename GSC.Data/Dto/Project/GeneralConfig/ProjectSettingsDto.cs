using GSC.Data.Entities.Common;


namespace GSC.Data.Dto.Project.GeneralConfig
{
    public class ProjectSettingsDto : BaseDto
    {
        public int ProjectId { get; set; }
        public bool IsCtms { get; set; }
        public bool IsEicf { get; set; }
        public bool IsScreening { get; set; }
        public bool IsPatientEngagement { get; set; }
    }
}
