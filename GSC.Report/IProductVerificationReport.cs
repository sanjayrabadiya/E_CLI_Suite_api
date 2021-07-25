using GSC.Data.Dto.Configuration;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Report
{
    public interface IProductVerificationReport
    {
        FileStreamResult GetProductVerificationSummary(ReportSettingNew reportSetting,int ProductReceiptId);
    }
}
