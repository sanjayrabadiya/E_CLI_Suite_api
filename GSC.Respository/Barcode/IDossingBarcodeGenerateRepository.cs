using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Barcode;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Barcode
{
    public interface IDossingBarcodeGenerateRepository : IGenericRepository<DossingBarcodeGenerate>
    {
        List<DossingBarcodeGenerateGridDto> GetReprintBarcodeGenerateData(int[] Ids);
        List<DossingBarcodeGenerateGridDto> GetBarcodeDetail(int attendanceId);
        string GetBarcodeString(DossingBarcode DossingBarcode, int number);
    }
}
