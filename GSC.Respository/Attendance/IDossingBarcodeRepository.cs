using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Barcode;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Attendance
{
    public interface IDossingBarcodeRepository : IGenericRepository<DossingBarcode>
    {
        List<DossingBarcodeGridDto> GetDossingBarcodeList(bool isDeleted);
        string Duplicate(DossingBarcodeDto objSave);
        string GenerateBarcodeString(DossingBarcode objSave);
        List<DossingBarcodeGridDto> UpdateBarcode(List<int> ids);
        void BarcodeReprint(List<int> ids);
        void DeleteBarcode(List<int> ids);
    }
}
