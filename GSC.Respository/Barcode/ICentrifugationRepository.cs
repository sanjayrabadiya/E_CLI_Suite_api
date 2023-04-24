using GSC.Common.GenericRespository;
using GSC.Data.Entities.Barcode;

namespace GSC.Respository.Barcode
{
    public interface ICentrifugationRepository : IGenericRepository<Centrifugation>
    {
        string Duplicate(BarcodeType objSave);
    }
}