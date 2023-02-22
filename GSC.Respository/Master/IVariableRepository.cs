using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IVariableRepository : IGenericRepository<Variable>
    {
        List<DropDownDto> GetVariableDropDown();
        List<VariableListDto> GetVariableListByDomainId(int domainId);
        string Duplicate(Variable objSave);
        IList<DropDownDto> GetColumnName(string tableName);
        List<VariableGridDto> GetVariableList(bool isDeleted);
        string NonChangeVariableCode(VariableDto variable);
    }
}