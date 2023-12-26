using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementKITSeriesRepository : IGenericRepository<SupplyManagementKITSeries>
    {
        void AddKitSeriesVisitDetail(SupplyManagementKITSeriesDto data);

        string GenerateKitSequenceNo(SupplyManagementKitNumberSettings kitsettings, int noseriese,SupplyManagementKITSeriesDto supplyManagementKITSeriesDto);

        string CheckExpiryDateSequenceWise(SupplyManagementKITSeriesDto supplyManagementKITSeriesDto);

        DateTime GetExpiryDateSequenceWise(SupplyManagementKITSeriesDto supplyManagementKITSeriesDto);

        string GenerateKitPackBarcode(SupplyManagementKITSeriesDto supplyManagementKitSeries);
    }
}