using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Barcode;

namespace GSC.Respository.Barcode
{
    public interface IBarcodeTypeRepository : IGenericRepository<BarcodeType>
    {
        List<DropDownDto> GetBarcodeTypeDropDown();
        string Duplicate(BarcodeType objSave);
    }
}