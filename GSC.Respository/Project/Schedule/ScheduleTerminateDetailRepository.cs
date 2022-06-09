using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Entities.Project.Schedule;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.Schedule
{
    public class ScheduleTerminateDetailRepository : GenericRespository<ScheduleTerminateDetail>, IScheduleTerminateDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;


        public ScheduleTerminateDetailRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
        }

        public ScheduleTerminateDetailDto GetDetailById(int ProjectScheduleTemplateId)
        {
            var result = All.Where(x => x.ProjectScheduleTemplateId == ProjectScheduleTemplateId && x.DeletedDate==null).Select(c => new ScheduleTerminateDetailDto
            {
                Id = c.Id,
                ProjectScheduleTemplateId = c.ProjectScheduleTemplateId,
                ProjectDesignTemplateId = c.ProjectDesignVariable.ProjectDesignTemplateId,
                ProjectDesignVisitId = c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisitId,
                ProjectDesignVariableId = c.ProjectDesignVariableId,
                Value = c.Value,
                ExtraData= _mapper.Map<List<ProjectDesignVariableValueDropDown>>(c.ProjectDesignVariable.Values.Where(b => b.DeletedDate == null).ToList()),
                DataType = c.ProjectDesignVariable.DataType,
                Operator=c.Operator
        }).FirstOrDefault();

            return result;

        }


    }
}
