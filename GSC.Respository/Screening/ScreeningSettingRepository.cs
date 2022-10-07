﻿using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Screening
{
    public class ScreeningSettingRepository : GenericRespository<ScreeningSetting>, IScreeningSettingRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ScreeningSettingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public ScreeningSettingDto GetProjectDefaultData()
        {
            var ScreeningSetting = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null)
                .Select(t => new ScreeningSettingDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    VisitId = t.VisitId,
                    StudyId = t.Project.ParentProjectId,
                    CountryId = t.Project.ManageSite.CompanyId,
                    UserId = t.UserId,
                }).FirstOrDefault();

            if (ScreeningSetting != null)
                if (_context.ProjectRight.Any(x => x.ProjectId == ScreeningSetting.ProjectId && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null))
                    return ScreeningSetting;
                else
                    return null;

            return ScreeningSetting;

        }
    }
}
