using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceArtificateRepository : IGenericRepository<ProjectWorkplaceArtificate>
    {
        List<DropDownDto> GetProjectWorkPlaceArtificateDropDown(int sectionId);
        List<WorkplaceFolderDto> GetWorkPlaceFolder(int EtmfArtificateMasterLbraryId, int ProjectWorkplaceArtificateId);
        WorkplaceChartDto GetRedChart(int id);
    }
}
