using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;

namespace GSC.Respository.Barcode
{
    public interface IBarcodeConfigRepository : IGenericRepository<BarcodeConfig>
    {
        List<BarcodeConfigGridDto> GetBarcodeConfig(bool isDeleted);
        List<BarcodeConfigDto> GetBarcodeConfigById(int id);
        string GenerateBarcodeString(int barcodeTypeId);
        BarcodeConfigDto GenerateBarcodeConfig(int barcodeTypeId);
        BarcodeConfig GetBarcodeConfig(int barcodeTypeId);
    }
}