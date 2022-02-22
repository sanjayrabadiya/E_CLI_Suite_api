using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
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
    public class StudyLevelFormRepository : GenericRespository<StudyLevelForm>, IStudyLevelFormRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public StudyLevelFormRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<StudyLevelFormGridDto> GetStudyLevelFormList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<StudyLevelFormGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(StudyLevelForm objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId && x.AppScreenId == objSave.AppScreenId
            && x.ActivityId == objSave.ActivityId && x.VariableTemplateId == objSave.VariableTemplateId && x.DeletedDate == null))
                return "Duplicate Form  : " + objSave.VariableTemplate.TemplateName;

            return "";
        }

        public IList<DropDownDto> GetTemplateDropDown(int projectId)
        {
            var template = All.Where(x => x.DeletedDate == null && x.ProjectId == projectId).Select(
                t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.VariableTemplate.TemplateName,
                    Code = t.VariableTemplate.TemplateCode
                }).ToList();

            return template;
        }
    }
}
