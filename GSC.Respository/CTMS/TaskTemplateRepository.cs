using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.CTMS
{
    public class TaskTemplateRepository : GenericRespository<TaskTemplate>, ITaskTemplateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public TaskTemplateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;

        }
        public List<DropDownDto> GetTaskTemplateDropDown()
        {
            return All.Where(x => x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.TemplateName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public List<TaskTemplateGridDto> GetStudyTrackerList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                 ProjectTo<TaskTemplateGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(TaskTemplate objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TemplateCode == objSave.TemplateCode.Trim() && x.DeletedDate == null))
                return "Duplicate Template Code : " + objSave.TemplateCode;

            return "";
        }
    }
}
