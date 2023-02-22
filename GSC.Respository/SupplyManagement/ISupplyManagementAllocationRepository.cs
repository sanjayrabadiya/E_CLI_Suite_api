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

        List<DropDownDto> GetPharmacyStudyProductTypeDropDown(int ProjectId);

        bool CheckRandomizationAssign(SupplyManagementAllocation obj);

        List<DropDownDto> GetTreatmentTypeKitSequence(int ProjectId);

        List<DropDownDto> GetPharmacyStudyProductTypeDropDownKitSequence(int ProjectId, string TreatmentType, int VisitId);

        
    }
}