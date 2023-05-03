using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsActionPointDto : BaseDto
    {
        public int CtmsMonitoringId { get; set; }
        public CtmsActionPointStatus? Status { get; set; }
        public string QueryDescription { get; set; }
        public string Response { get; set; }
        public int? ResponseBy { get; set; }
        public DateTime? ResponseDate { get; set; }
        public int? CloseBy { get; set; }
        public DateTime? CloseDate { get; set; }
    }

    public class CtmsActionPointGridDto : BaseAuditDto
    {
        public int CtmsMonitoringId { get; set; }
        public CtmsActionPointStatus? Status { get; set; }
        public string StatusName { get; set; }
        public string QueryDescription { get; set; }
        public DateTime QueryDate { get; set; }
        public string QueryBy { get; set; }
        public string Response { get; set; }
        public string ResponseBy { get; set; }
        public DateTime? ResponseDate { get; set; }
        public string CloseBy { get; set; }
        public DateTime? CloseDate { get; set; }
        public string Activity { get; set; }
        public string Site { get; set; }
        public int? AvgOpenQueries { get; set; }
    }
}
