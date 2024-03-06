using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System;


namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementKitSeriesRepository : IGenericRepository<SupplyManagementKITSeries>
    {
        void AddKitSeriesVisitDetail(SupplyManagementKITSeriesDto data);

        string GenerateKitSequenceNo(SupplyManagementKitNumberSettings kitsettings, int noseriese,SupplyManagementKITSeriesDto supplyManagementKITSeriesDto);

        string CheckExpiryDateSequenceWise(SupplyManagementKITSeriesDto supplyManagementKITSeriesDto);

        DateTime? GetExpiryDateSequenceWise(SupplyManagementKITSeriesDto supplyManagementKITSeriesDto);

        string GenerateKitPackBarcode(SupplyManagementKITSeriesDto supplyManagementKitSeries);
    }
}