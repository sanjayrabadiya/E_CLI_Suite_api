using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Barcode;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Attendance
{
    public interface ISampleBarcodeRepository : IGenericRepository<SampleBarcode>
    {
        List<SampleBarcodeGridDto> GetSampleBarcodeList(bool isDeleted);
        string Duplicate(SampleBarcodeDto objSave);
        string GenerateBarcodeString(SampleBarcodeDto objSave);
        void UpdateBarcode(List<int> ids);
        void BarcodeReprint(List<int> ids);
        void DeleteBarcode(List<int> ids);
        List<ProjectDropDown> GetProjectDropdown();
        List<ProjectDropDown> GetChildProjectDropDown(int parentProjectId);
        List<DropDownDto> GetVisitList(int projectId, int siteId);
        List<DropDownDto> GetTemplateList(int projectId, int siteId, int visitId);
        List<DropDownDto> GetVolunteerList(int projectId, int siteId, int visitId, int templateId);
    }
}
