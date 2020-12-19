using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.UserMgt
{
    public class AppScreenPatientRightsDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int AppScreenPatientId { get; set; }
        public string AppScreenPatientScreenName { get; set; }
        public bool IsChecked { get; set; }
        
    }

    public class AppScreenPatientRightsGridDto
    {
        public int ProjectId { get; set; }
        public string StudyName { get; set; }
        public string PatientModules { get; set; }
    }
}
