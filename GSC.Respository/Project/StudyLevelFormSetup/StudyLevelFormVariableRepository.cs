using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.StudyLevelFormSetup
{
    public class StudyLevelFormVariableRepository : GenericRespository<StudyLevelFormVariable>, IStudyLevelFormVariableRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public StudyLevelFormVariableRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public string Duplicate(StudyLevelFormVariable objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.VariableCode == objSave.VariableCode &&
                x.StudyLevelFormId == objSave.StudyLevelFormId && x.DeletedDate == null))
                return "Duplicate Variable code : " + objSave.VariableCode;

            if (All.Any(x => x.Id != objSave.Id && x.VariableName == objSave.VariableName &&
                             x.StudyLevelFormId == objSave.StudyLevelFormId &&
                             x.DomainId == objSave.DomainId && x.AnnotationTypeId == objSave.AnnotationTypeId &&
                             x.DeletedDate == null))
                return "Duplicate Record : " + objSave.VariableName;

            if (All.Any(x =>
                x.Id != objSave.Id && x.DomainId == objSave.DomainId &&
                x.StudyLevelFormId == objSave.StudyLevelFormId && x.Annotation == objSave.Annotation &&
                !string.IsNullOrEmpty(x.Annotation) && x.DeletedDate == null))
                return "Duplicate Variable Annotation: " + objSave.Annotation;

            if (All.Any(x => x.Id != objSave.Id && x.DomainId == objSave.DomainId &&
                             x.StudyLevelFormId == objSave.StudyLevelFormId &&
                             x.VariableAlias == objSave.VariableAlias && !string.IsNullOrEmpty(x.VariableAlias) &&
                             x.DeletedDate == null)) return "Duplicate Variable Alias: " + objSave.VariableAlias;

            return "";
        }

        public IList<StudyLevelFormVariableBasicDto> GetVariabeBasic(int studyLevelFormId)
        {
            return All.Where(x => x.DeletedDate == null && x.StudyLevelFormId == studyLevelFormId)
                .Select(c => new StudyLevelFormVariableBasicDto
                {
                    Id = c.Id,
                    Value = c.VariableName,
                    DesignOrder = c.DesignOrder,
                }).OrderBy(o => o.DesignOrder).ToList();
        }
    }
}
