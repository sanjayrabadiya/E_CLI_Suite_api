using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Master
{
    public class TableFieldName : BaseEntity, ICommonAduit
    {
        public string TableName { get; set; }

        public string FieldName { get; set; }

        public string LabelName { get; set; }
    }
}
