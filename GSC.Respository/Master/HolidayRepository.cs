﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class HolidayRepository : GenericRespository<Holiday>, IHolidayRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public HolidayRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }


        public IList<HolidayGridDto> GetHolidayList(int Id, bool isDeleted)
        {
            return All.Where(x => x.InvestigatorContactId == Id && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                      ProjectTo<HolidayGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string DuplicateHoliday(Holiday objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.InvestigatorContactId == objSave.InvestigatorContactId && x.HolidayName == objSave.HolidayName.Trim() && x.DeletedDate == null))
                return "Duplicate Holiday : " + objSave.HolidayName;

            return "";
        }

        public List<DropDownDto> GetHolidayDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.HolidayName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
