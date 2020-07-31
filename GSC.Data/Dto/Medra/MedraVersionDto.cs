using GSC.Data.Entities.Common;
using GSC.Data.Entities.Medra;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MedraVersionDto : BaseDto
    {
        public int DictionaryId { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string DictionaryName { get; set; }
        public Dictionary Dictionary { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }

        //public int CreatedBy { get; set; }
        //public int? DeletedBy { get; set; }
        //public int? ModifiedBy { get; set; }
        //public System.DateTime? CreatedDate { get; set; }
        //public System.DateTime? ModifiedDate { get; set; }
        //public System.DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        //public string CompanyName { get; set; }

    }
}
