using GSC.Common.GenericRespository;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public interface IEconsentSetupRepository : IGenericRepository<EconsentSetup>
    {
        string Duplicate(EconsentSetupDto objSave);
        List<DropDownDto> GetEconsentDocumentDropDown(int projectId);
        List<DropDownDto> GetPatientStatusDropDown();
        List<EconsentSetupGridDto> GetEconsentSetupList(int projectid,bool isDeleted);
        EconsentSetupDto GetEconsent(int id);
        int AddEconsentSetup(EconsentSetupDto econsentSetupDto);
        int UpdateEconsentSetup(EconsentSetupDto econsentSetupDto);
        string validatebeforeadd(EconsentSetupDto econsentSetupDto);
    }
}
