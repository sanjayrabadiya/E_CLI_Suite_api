﻿using GSC.Common.GenericRespository;
using System.Collections.Generic;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Dto.CTMS;

namespace GSC.Respository.CTMS
{
    public interface IResourceTypeRepository : IGenericRepository<ResourceType>
    {
        string Duplicate(ResourceType objSave);
        List<ResourceTypeGridDto> GetResourceTypeList(bool isDeleted);
        List<DropDownDto> GetUnitTypeDropDown();
        List<DropDownDto> GetDesignationDropDown();
        List<DropDownDto> GetDesignationDropDown(int resourceTypeID, int resourceSubTypeID, int projectId);
        List<DropDownDto> GetNameOfMaterialDropDown(int resourceTypeID, int resourceSubTypeID);
        List<DropDownDto> GetRollUserDropDown(int designationID, int projectId);
        List<DropDownDto> GetCurrencyDropDown();
        string ResourceWorking(int id);
    }
}