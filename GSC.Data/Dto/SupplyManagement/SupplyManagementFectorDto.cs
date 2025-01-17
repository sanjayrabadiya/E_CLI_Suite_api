﻿using GSC.Data.Entities.Common;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementFectorDto : BaseDto
    {
        public int ProjectId { get; set; }
        public string Formula { get; set; }
        public string ProjectCode { get; set; }

        public string CheckFormula { get; set; }

        public string SourceFormula { get; set; }

        public string SampleResult { get; set; }

        public string ErrorMessage { get; set; }


        public List<SupplyManagementFectorDetailDto> Children { get; set; }
    }

    public class SupplyManagementFectorGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public string Formula { get; set; }
        public string ProjectCode { get; set; }
        public string SourceFormula { get; set; }

        public string IpAddress { get; set; }
        public string TimeZone { get; set; }

    }

    public class FactorCheckResult
    {
        public int Id { get; set; }
        public bool IsValid { get; set; }
        public string SampleText { get; set; }
        public string Result { get; set; }
        public string ResultMessage { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string ProductType { get; set; }

    }
}
