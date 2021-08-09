using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Master
{
    public class TableFieldNameDto : BaseDto
    {
        public string TableName { get; set; }

        public string FieldName { get; set; }

        public string LabelName { get; set; }
    }
}
