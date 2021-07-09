using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GSC.Common.WorkingDayHelper;

namespace GSC.Respository.CTMS
{
    public class WeekEndMasterRepository : GenericRespository<WeekEndMaster>, IWeekEndMasterRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public WeekEndMasterRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<WeekEndGridDto> GetWeekendList(bool isDeleted)
        {
            var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<WeekEndGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            var data = result.Select(r =>
            {
                r.ProjectCode = r.IsSite == true ? _context.Project.Find(Convert.ToInt32(r.ProjectCode)).ProjectCode : r.ProjectCode;
                return r;
            }).ToList();

            return data;
        }

        public List<WeekendData> GetworkingDayList(int ProjectId)
        {
            var weekend = All.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).ToList();
            var weekendlis = new List<WeekendData>();
            //if (weekend != null)
            //{
            //    if (weekend.Sunday == true)
            //        weekendlis.Add("Sunday");
            //    if (weekend.Monday == true)
            //        weekendlis.Add("Monday");
            //    if (weekend.Tuesday == true)
            //        weekendlis.Add("Tuesday");
            //    if (weekend.Wednesday == true)
            //        weekendlis.Add("Wednesday");
            //    if (weekend.Thursday == true)
            //        weekendlis.Add("Thursday");
            //    if (weekend.Friday == true)
            //        weekendlis.Add("Friday");
            //    if (weekend.Saturday == true)
            //        weekendlis.Add("Saturday");
            //}
            foreach (var item in weekend)
            {
                WeekendData obj = new WeekendData();
                obj.Weekend = item.AllWeekOff.ToString();
                obj.Frequency = ((FrequencyType)item.Frequency).GetDescription();
                weekendlis.Add(obj);
            }
            return weekendlis;
        }

        public List<string> GetweekEndDay(int ProjectId)
        {
            var weekend = All.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).ToList();
            var weekendlis = new List<string>();

            var days = new List<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            //if (weekend != null)
            //{
            //    if (weekend.Sunday == false)
            //        weekendlis.Add("Sunday");
            //    if (weekend.Monday == false)
            //        weekendlis.Add("Monday");
            //    if (weekend.Tuesday == false)
            //        weekendlis.Add("Tuesday");
            //    if (weekend.Wednesday == false)
            //        weekendlis.Add("Wednesday");
            //    if (weekend.Thursday == false)
            //        weekendlis.Add("Thursday");
            //    if (weekend.Friday == false)
            //        weekendlis.Add("Friday");
            //    if (weekend.Saturday == false)
            //        weekendlis.Add("Saturday");
            //}

            weekendlis = days.Where(x => !weekend.Select(y => y.AllWeekOff.ToString()).Contains(x)).ToList();
            return weekendlis;
        }
    }
}
