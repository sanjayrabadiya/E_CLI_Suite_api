using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;

namespace GSC.Respository.Medra
{
    public interface IMedraVersionRepository: IGenericRepository<MedraVersion>
    {
        string Duplicate(MedraVersion objSave);
        List<DropDownDto> GetMedraVersionDropDown();
        List<MedraVersionGridDto> GetMedraVersionList(bool isDeleted);
    }
}