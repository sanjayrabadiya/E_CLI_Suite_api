using GSC.Common.GenericRespository;
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
    public class ScheduleTerminateDetailsRepository : GenericRespository<ScheduleTerminateDetails>, IScheduleTerminateDetailsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;


        public ScheduleTerminateDetailsRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public ScheduleTerminateDetailsDto GetDetailById(int ProjectScheduleTemplateId)
        {
            var result = All.Where(x => x.ProjectScheduleTemplateId == ProjectScheduleTemplateId).Select(c => new ScheduleTerminateDetailsDto
            {
                Id = c.Id,
                ProjectScheduleTemplateId = c.ProjectScheduleTemplateId,
                ProjectDesignTemplateId = c.ProjectDesignVariable.ProjectDesignTemplateId,
                ProjectDesignVisitId = c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisitId,
                ProjectDesignVariableId = c.ProjectDesignVariableId,
                Value = c.Value
            }).FirstOrDefault();

            return result;

        }


    }
}
