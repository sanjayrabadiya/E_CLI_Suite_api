﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class WorkingDayRepository : GenericRespository<WorkingDay>, IWorkingDayRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public WorkingDayRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }
        public List<WorkingDayListDto> GetWorkingDayList(bool isDeleted)
        {
            var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).
            ProjectTo<WorkingDayListDto>(_mapper.ConfigurationProvider).ToList();
            var data = result.Select(r =>
            {
                var proID = r.IsSite == true ? _context.Project.Where(x=>x.Id == r.ProjectId).Select(s=>s.ParentProjectId).FirstOrDefault() : null;
                if(proID!=null)
                { 
                     r.SiteCode = _context.Project.Where(x => x.Id == proID).Select(s => s.ProjectCode).FirstOrDefault();
                }
                else
                { 
                    r.SiteCode = r.ProjectCode;
                    r.ProjectCode = null;
                }
                return r;
            }).ToList();

            return data;
        }
    }
}
