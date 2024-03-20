using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Barcode;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Barcode
{
    public interface IPkBarcodeGenerateRepository : IGenericRepository<PkBarcodeGenerate>
    {
        List<PkBarcodeGenerateGridDto> GetReprintBarcodeGenerateData(int[] Ids);
        List<PkBarcodeGenerateGridDto> GetBarcodeDetail(int PkBarcodeId);
        string GetBarcodeString(PKBarcode pKBarcode, int number);
    }
}
