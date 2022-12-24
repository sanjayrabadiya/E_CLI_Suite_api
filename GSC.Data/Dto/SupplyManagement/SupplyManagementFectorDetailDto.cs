using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementFectorDetailDto : BaseDto
    {
        public int SupplyManagementFectorId { get; set; }
        public Fector Fector { get; set; }
        public FectorOperator Operator { get; set; }
        public string ProductTypeCode { get; set; }
        public string CollectionValue { get; set; }
        public string LogicalOperator { get; set; }
        public int? Ratio { get; set; }
        public string FactoreName { get; set; }
        public string FactoreOperatorName { get; set; }
        public string collectionValueName { get; set; }
        public FectoreType? Type { get; set; }
        public string TypeName { get; set; }

        public string ProjectCode { get; set; }
        public int ProjectId { get; set; }

        public string InputValue { get; set; }
        public string QueryFormula { get; set; }

        public DataType? dataType { get; set; }
    }
}
