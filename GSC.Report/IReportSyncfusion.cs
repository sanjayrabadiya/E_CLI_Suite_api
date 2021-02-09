﻿using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Report;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Report
{
    public interface IReportSyncfusion
    {
        void BlankReportGenerate(ReportSettingNew reportSetting, JobMonitoring jobMonitoring);
        void DataGenerateReport(ReportSettingNew reportSetting, JobMonitoring jobMonitoring);
        FileStreamResult GetProjectDesign(ReportSettingNew reportSetting);

    }
}
