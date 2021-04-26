﻿using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public List<string> GetworkingDayList(int ProjectId)
        {
            var weekend = All.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).SingleOrDefault();
            var weekendlis = new List<string>();
            if (weekend != null)
            {                
                if (weekend.Sunday == true)
                    weekendlis.Add("Sunday");
                if (weekend.Monday == true)
                    weekendlis.Add("Monday");
                if (weekend.Tuesday == true)
                    weekendlis.Add("Tuesday");
                if (weekend.Wednesday == true)
                    weekendlis.Add("Wednesday");
                if (weekend.Thursday == true)
                    weekendlis.Add("Thursday");
                if (weekend.Friday == true)
                    weekendlis.Add("Friday");
                if (weekend.Saturday == true)
                    weekendlis.Add("Saturday");
            }
            return weekendlis;
        }
    }
}