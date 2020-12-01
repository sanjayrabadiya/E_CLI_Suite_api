using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Master
{
    public class DashboardPatientDto
    {
        public int projectId { get; set; }
        public string studycode { get; set; }
        public string sitecode { get; set; }
        public string studyname { get; set; }
        public string sitename { get; set; }
        public int patientStatusId { get; set; }
        public string patientStatus { get; set; }
        public string investigatorName { get; set; }
        public string investigatorcontact { get; set; }
        public string investigatorEmail { get; set; }
    }
}
