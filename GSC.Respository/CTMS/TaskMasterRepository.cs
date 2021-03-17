using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.CTMS
{
    public class TaskMasterRepository : GenericRespository<TaskMaster>, ITaskMasterRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public TaskMasterRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<TaskMasterGridDto> GetTasklist(bool isDeleted, int templateId)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.TaskTemplateId == templateId).OrderBy(x => x.TaskOrder).
                   ProjectTo<TaskMasterGridDto>(_mapper.ConfigurationProvider).ToList();

        }
        public int UpdateTaskOrder(TaskmasterDto taskmasterDto)
        {
            if (taskmasterDto.Position == "above")
            {
                var data = All.Where(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.TaskOrder >= taskmasterDto.TaskOrder && x.DeletedDate == null).ToList();
                foreach (var item in data)
                {
                    item.TaskOrder = ++item.TaskOrder;
                    Update(item);
                }
                return taskmasterDto.TaskOrder;
            }
            if (taskmasterDto.Position == "below")
            {
                var data = All.Where(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.TaskOrder > taskmasterDto.TaskOrder && x.DeletedDate == null).ToList();
                foreach (var item in data)
                {
                    item.TaskOrder = ++item.TaskOrder;
                    Update(item);
                }
                return ++taskmasterDto.TaskOrder;
            }
            else
            {
                var count = All.Where(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.DeletedDate == null).Count();
                return ++count;
            }

        }
    }
}
