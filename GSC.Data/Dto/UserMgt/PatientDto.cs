using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.UserMgt
{
    public class PatientDto
    {
        public bool IsDeleted{ get; set; }
        public int ParentProjectId { get; set; }
        public int ProjectId { get; set;}
    }
}
