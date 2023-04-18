using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System.Collections.Generic;

namespace GSC.Respository.Master
{
    public interface IIecirbRepository : IGenericRepository<Iecirb>
    {
        string Duplicate(Iecirb objSave);
        List<DropDownDto> GetIecirbDropDown();
        void AddSiteAddress(IecirbDto iecirbDto);
        void UpdateSiteAddress(IecirbDto iecirbDto);
    }
}