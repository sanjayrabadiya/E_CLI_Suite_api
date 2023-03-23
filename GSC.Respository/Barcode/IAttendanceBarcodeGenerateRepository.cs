using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Barcode
{
    public interface IAttendanceBarcodeGenerateRepository : IGenericRepository<AttendanceBarcodeGenerate>
    {
        List<AttendanceBarcodeGenerateGridDto> GetReprintBarcodeGenerateData(int[] Ids);
        List<AttendanceBarcodeGenerateGridDto> GetBarcodeDetail(int attendanceId);
        string GetBarcodeString(int id);
    }
}
