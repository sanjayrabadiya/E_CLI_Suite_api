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
        FileStreamResult GetProjectDesign(ReportSettingNew reportSetting);
        string DossierPdfReportGenerate(ReportSettingNew reportSetting, JobMonitoring jobMonitoring);
        string ScreeningPdfReportGenerate(ScreeningReportSetting reportSetting, JobMonitoring jobMonitoring);
    }
}
