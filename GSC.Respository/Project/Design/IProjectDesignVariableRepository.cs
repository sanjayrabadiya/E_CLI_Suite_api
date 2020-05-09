using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Design;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVariableRepository : IGenericRepository<ProjectDesignVariable>
    {
        IList<DropDownDto> GetVariabeDropDown(int projectDesignTemplateId);
        string Duplicate(ProjectDesignVariable objSave);

        IList<DropDownVaribleDto> GetAnnotationDropDown(int projectDesignId, bool isFormula);

        IList<DropDownVaribleDto> GetVariabeAnnotationDropDown(int projectDesignTemplateId, bool isFormula);

        //Added method By Vipul 19022020
        IList<DropDownDto> GetVariabeAnnotationDropDownForProjectDesign(int projectDesignTemplateId);

        IList<DropDownVaribleDto> GetTargetVariabeAnnotationDropDown(int projectDesignTemplateId);

        IList<DropDownVaribleAnnotationDto> GetVariabeAnnotationByDomainDropDown(int domainId, int projectId);
    }
}