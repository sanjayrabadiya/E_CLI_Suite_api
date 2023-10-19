using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace GSC.Respository.CTMS
{
    public class TaskTemplateRepository : GenericRespository<TaskTemplate>, ITaskTemplateRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public TaskTemplateRepository(IGSCContext context,
            IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _context = context;

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

        //add by mitul on 03-10-2023 #GS1-I3054
        public string AlreadyUSed(int id)
        {
            if (_context.StudyPlan.Where(x=>x.TaskTemplateId == id && x.DeletedDate==null).Count()>0)
                return "Tracker already in use - Should not be Deleted";

            return "";
        }
    }
}
