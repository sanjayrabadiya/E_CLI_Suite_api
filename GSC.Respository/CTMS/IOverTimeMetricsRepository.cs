﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;
namespace GSC.Respository.CTMS
{
    public interface IOverTimeMetricsRepository : IGenericRepository<OverTimeMetrics>
    {
        List<OverTimeMetricsGridDto> GetTasklist(bool isDeleted, int templateId, int projectId, int countryId, int siteId);
        //string Duplicate(OverTimeMetrics objSave);
        string PlannedCheck (OverTimeMetrics objSave);
        string UpdatePlanning(OverTimeMetrics objSave);

        string UpdateActualNo(OverTimeMetrics objSave);
        List<ProjectDropDown> GetChildProjectWithParentProjectDropDown(int parentProjectId);
    }
}