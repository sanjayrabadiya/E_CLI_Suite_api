using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IETMFWorkplaceRepository : IGenericRepository<ProjectWorkplace>
    {
        List<ETMFWorkplaceGridDto> GetETMFWorkplaceList(bool isDeleted);
        string Duplicate(int id);
        //List<TreeValue> Get(int id);
        List<TreeValue> GetTreeview(int id, EtmfChartType? chartType);
        ProjectWorkplace SaveFolderStructure(Data.Entities.Master.Project Project, List<ProjectDropDown> childProjectList, List<DropDownDto> countryList, List<MasterLibraryJoinDto> artificiteList,string imageUrl);
        ProjectWorkplace SaveSiteFolderStructure(Data.Entities.Master.Project projectDetail, List<int> childProjectList, List<DropDownDto> countryList, List<MasterLibraryJoinDto> artificiteList, string docPath);
        byte[] CreateZipFileOfWorkplace(int Id);
        List<ChartReport> GetChartReport(int id, EtmfChartType? chartType);
    }
}
