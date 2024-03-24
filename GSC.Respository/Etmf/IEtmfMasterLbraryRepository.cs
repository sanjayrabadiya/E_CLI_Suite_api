using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IEtmfMasterLbraryRepository : IGenericRepository<EtmfMasterLibrary>
    {
        string Duplicate(EtmfMasterLibrary objSave);
        List<DropDownDto> GetSectionMasterLibraryDropDown(int EtmfZoneMasterLibraryId);
        List<EtmfMasterLibrary> ExcelDataConvertToEntityformat(List<MasterLibraryDto> data);
        List<DropDownDto> GetZoneMasterLibraryDropDown(string version);
        PdfPageTemplateElement AddFooter(PdfDocument doc, string documentName, string version);
    }
}
