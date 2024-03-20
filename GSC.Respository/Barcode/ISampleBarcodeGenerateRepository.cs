using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Barcode;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Barcode
{
    public interface ISampleBarcodeGenerateRepository : IGenericRepository<SampleBarcodeGenerate>
    {
        List<SampleBarcodeGenerateGridDto> GetReprintBarcodeGenerateData(int[] Ids);
        List<SampleBarcodeGenerateGridDto> GetBarcodeDetail(int SampleBarcodeId);
        string GetBarcodeString(SampleBarcode SampleBarcode, int number);
    }
}
