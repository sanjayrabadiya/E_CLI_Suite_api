using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;

namespace GSC.Respository.Barcode
{
    public interface IPharmacyBarcodeConfigRepository : IGenericRepository<PharmacyBarcodeConfig>
    {
        List<PharmacyBarcodeConfigGridDto> GetBarcodeConfig(bool isDeleted);
        List<PharmacyBarcodeConfigDto> GetBarcodeConfigById(int id);
        string GenerateBarcodeString(int barcodeTypeId);
        PharmacyBarcodeConfigDto GenerateBarcodeConfig(int barcodeTypeId);
        PharmacyBarcodeConfig GetBarcodeConfig(int barcodeTypeId);

        string ValidateBarcodeConfig(PharmacyBarcodeConfig pharmacyBarcodeConfig);
    }
}