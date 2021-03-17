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
    public class StudyPlanRepository : GenericRespository<StudyPlan>, IStudyPlanRepository
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public StudyPlanRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<StudyPlanGridDto> GetStudyplanList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).
                   ProjectTo<StudyPlanGridDto>(_mapper.ConfigurationProvider).ToList();

        }

        //public void InsertMasterTask(int StudyPlanId,int TaskTemplateId)
        //{
        //    var taskdata = _context.TaskMaster.Where(x => x.TaskTemplateId == TaskTemplateId)
        //        .ProjectTo<StudyPlanTask>(_mapper.ConfigurationProvider).ToList();

        //    DateTime startdate = DateTime.Now;
        //    //TimeSpan duration = new TimeSpan(1, 0, 0, 0);
        //    //DateTime answer = today.Add(duration);
        //    taskdata.ForEach(x =>
        //    {
        //        x.StartDate = startdate;
        //        x.EndDate = startdate.Add(new TimeSpan(1, 0, 0, 0));
        //        startdate = startdate.Add(new TimeSpan(1, 0, 0, 0));
        //    });
        //    _context.StudyPlanTask.UpdateRange(taskdata);
        //    _context.Save();
        //    //foreach (var item in taskdata)
        //    //{
        //    //    var task = _mapper.Map<StudyPlanTask>(item);
        //    //    task.StudyPlanId = studyplan.Id;
        //    //    task.StartDate = DateTime.Now;
        //    //    task.EndDate = DateTime.Now;
        //    //    task.isMileStone = true;
        //    //    task.Progress = 80;
        //    //    //_studyPlanTaskRepository.Add(task);
        //    //    _uow.Save();
        //    //}
        //}
    }
}
