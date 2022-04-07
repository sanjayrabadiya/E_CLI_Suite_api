using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class ProjectRemoveDataDto
    {
        public string ConnectionString { get; set; }

        public int ProjectId { get; set; } 
        
    }
    public class ProjectRemoveDataSuccess
    {
        public string Message { get; set; }

        public bool IsSuccess { get; set; }

    }
    public class Companystudyconfig
    {
        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public int NoofStudy { get; set; }
    }
}