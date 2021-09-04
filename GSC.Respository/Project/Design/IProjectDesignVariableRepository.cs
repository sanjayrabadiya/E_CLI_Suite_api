using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVariableRepository : IGenericRepository<ProjectDesignVariable>
    {
        IList<DropDownDto> GetVariabeDropDown(int projectDesignTemplateId);
        string Duplicate(ProjectDesignVariable objSave);

        IList<DropDownVaribleDto> GetAnnotationDropDown(int projectDesignId, bool isFormula);

        IList<DropDownVaribleDto> GetVariabeAnnotationDropDown(int projectDesignTemplateId, bool isFormula);

        IList<DropDownVaribleAnnotationDto> GetVariabeAnnotationByDomainDropDown(int domainId, int projectId);

        IList<DropDownVaribleDto> GetTargetVariabeAnnotationForScheduleDropDown(int projectDesignTemplateId);

        //Added method By Vipul 22092020 for visit status in project design get only date and datetime variable
        IList<DropDownVaribleDto> GetVariabeAnnotationDropDownForVisitStatus(int projectDesignTemplateId);
        IList<DropDownDto> GetVariableByMultipleTemplateDropDown(int?[] templateIds);

        ProjectDesignVariableRelationDto GetProjectDesignVariableRelation(int id);
        IList<ProjectDesignVariableBasicDto> GetVariabeBasic(int projectDesignTemplateId, CheckVersionDto checkVersion);

    }
}