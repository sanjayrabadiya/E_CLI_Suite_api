﻿using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.PropertyMapping;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class StudyScopingRepository : GenericRespository<StudyScoping, GscContext>, IStudyScopingRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public StudyScopingRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(uow, jwtTokenAccesser)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public string Duplicate(StudyScoping objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId && x.ProjectDesignVariableId == objSave.ProjectDesignVariableId && x.DeletedDate == null))
            {
                return "Duplicate Variable name : " + Context.ProjectDesignVariable.Where(p => p.Id == objSave.ProjectDesignVariableId).FirstOrDefault().VariableName;
            }
            return "";
        }

        public List<StudyScopingDto> GetStudyScopingList(int projectId)
        {
            try
            {
                var result = All.Where(r => r.ProjectId == projectId && r.DeletedDate == null).Select(x =>
                        new StudyScopingDto
                        {
                            Id = x.Id,
                            ProjectId = x.ProjectId,
                            ProjectName = x.Project.ProjectName,
                            TemplateId = Context.ProjectDesignVariable.Where(p => p.Id == x.ProjectDesignVariable.Id).FirstOrDefault().ProjectDesignTemplateId,
                            TemplateName = Context.ProjectDesignVariable.Where(p => p.Id == x.ProjectDesignVariable.Id).FirstOrDefault().ProjectDesignTemplate.TemplateName,
                            VisitId = Context.ProjectDesignVariable.Where(p => p.Id == x.ProjectDesignVariable.Id).FirstOrDefault().ProjectDesignTemplate.ProjectDesignVisitId,
                            VisitName = x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                            ProjectDesignPeriodId = x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId,
                            ProjectDesignPeriodName = x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                            IsByAnnotation = x.IsByAnnotation,
                            ScopingBy = x.IsByAnnotation ? 2 : 1,
                            ProjectDesignVariableId = x.ProjectDesignVariableId,
                            VariableAnnotation = x.IsByAnnotation ? x.ProjectDesignVariableId : 0,
                            VariableName = Context.ProjectDesignVariable.Where(p => p.Id == x.ProjectDesignVariableId).FirstOrDefault().VariableName,
                            DomainId = x.DomainId,
                            DomainName = x.Domain.DomainName,                            
                            MedraConfigId = x.MedraConfigId,
                            VersionName = x.MedraConfig.MedraVersion.Dictionary.DictionaryName+"-"+ x.MedraConfig.Language.LanguageName + "-"+ x.MedraConfig.MedraVersion.Version,                            
                            CoderProfile = x.CoderProfile,
                            CoderApprover = x.CoderApprover,
                            CoderProfileName = Context.SecurityRole.Where(s => s.Id == x.CoderProfile && s.DeletedDate == null).FirstOrDefault().RoleName,
                            CoderApproverName = Context.SecurityRole.Where(s => s.Id == x.CoderApprover && s.DeletedDate == null).FirstOrDefault().RoleName,
                            FieldName = x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName + "." +
                                x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName + "." +
                                x.ProjectDesignVariable.ProjectDesignTemplate.TemplateName + "." + x.ProjectDesignVariable.VariableName
                        }).OrderBy(x => x.Id).ToList();

                return result;
            }
            catch (Exception ex)
           {
                throw ex;
            }
        }
    }
}
