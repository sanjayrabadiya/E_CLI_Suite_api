using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode.Generate;
using GSC.Data.Entities.Barcode.Generate;

namespace GSC.Respository.Barcode.Generate
{
    public interface IBarcodeSubjectDetailRepository : IGenericRepository<BarcodeSubjectDetail>
    {
        List<BarcodeSubjectDetailDto> GetBarcodeSubjectDetail(bool isDeleted);
    }
}