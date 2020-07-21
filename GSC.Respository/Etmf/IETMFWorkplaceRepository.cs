using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IETMFWorkplaceRepository : IGenericRepository<ProjectWorkplace>
    {

        string Duplicate(int id);
        //List<TreeValue> Get(int id);
        List<TreeValue> GetTreeview(int id);
        ProjectWorkplace SaveFolderStructure(Data.Entities.Master.Project Project, List<ProjectDropDown> childProjectList, List<DropDownDto> countryList, List<MasterLibraryJoinDto> artificiteList,string imageUrl);
    }
}
