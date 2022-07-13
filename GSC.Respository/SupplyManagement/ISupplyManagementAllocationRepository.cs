using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementAllocationRepository : IGenericRepository<SupplyManagementAllocation>
    {
        List<SupplyManagementAllocationGridDto> GetSupplyAllocationList(bool isDeleted, int ProjectId);
        IList<DropDownDto> GetVisitDropDownByRandomization(int projectId);

        List<DropDownDto> GetParentProjectDropDownProjectRight();

        IList<DropDownDto> GetTemplateDropDownByVisitId(int visitId);

        IList<DropDownDto> GetVariableDropDownByTemplateId(int templateId);

        object GetProductTypeByVisit(int visitId);

        string CheckDuplicate(SupplyManagementAllocation obj);
    }
}