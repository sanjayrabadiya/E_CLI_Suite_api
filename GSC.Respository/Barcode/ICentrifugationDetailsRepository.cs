using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using System.Collections.Generic;

namespace GSC.Respository.Barcode
{
    public interface ICentrifugationDetailsRepository : IGenericRepository<CentrifugationDetails>
    {
        List<CentrifugationDetailsGridDto> GetCentrifugationDetails(int siteId);
        List<CentrifugationDetailsGridDto> GetCentrifugationDetailsByPKBarcode(string PkBarcodeString);
        void StartCentrifugation(List<int> ids);
        void StartReCentrifugation(ReCentrifugationDto dto);
    }
}