using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.UserMgt
{
    public class ReportScreenDto : BaseDto
    {
        public int Id { get; set; }
        public string ReportCode { get; set; }
        public string ReportName { get; set; }
        public string ReportGroup { get; set; }
        public bool IsFavourite { get; set; }

        public int? ParentAppScreenId { get; set; }

        public bool IsMenu { get; set; }

        public bool IsPermission { get; set; }

        public bool IsView { get; set; }

        public bool IsAdd { get; set; }

        public bool IsEdit { get; set; }

        public bool IsDelete { get; set; }

        public bool IsExport { get; set; }

        public string UrlName { get; set; }

        public int SeqNo { get; set; }
        public string IconPath { get; set; }

        public bool? IsMaster { get; set; }
        public string TableName { get; set; }
    }
}
