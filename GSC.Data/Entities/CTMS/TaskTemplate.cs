using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class TaskTemplate : BaseEntity, ICommonAduit
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
    }
}
