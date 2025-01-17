﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceArtificateRepository : IGenericRepository<EtmfProjectWorkPlace>
    {
        List<DropDownDto> GetProjectWorkPlaceArtificateDropDown(int sectionId);
        List<WorkplaceFolderDto> GetWorkPlaceFolder(int EtmfArtificateMasterLbraryId, int ProjectWorkplaceArtificateId);
        WorkplaceChartDto GetDocChart(WorkplaceChartFilterDto filters);
        int ClosestToNumber(List<int?> collection, int target);
    }
}
