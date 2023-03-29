using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Entities.Attendance;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Attendance
{
    public interface IPKBarcodeRepository : IGenericRepository<PKBarcode>
    {
        List<PKBarcodeGridDto> GetPKBarcodeList(bool isDeleted);
        string Duplicate(PKBarcodeDto objSave);
        string GenerateBarcodeString(PKBarcodeDto objSave);
        void UpdateBarcode(List<int> ids);
        void BarcodeReprint(List<int> ids);
        void DeleteBarcode(List<int> ids);
        List<BarcodeDataEntrySubject> GetPkSubjectDetails(int siteId, int templateId);
    }
}
