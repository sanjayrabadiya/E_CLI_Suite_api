﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignTemplateNoteRepository : GenericRespository<ProjectDesignTemplateNote>, IProjectDesignTemplateNoteRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public ProjectDesignTemplateNoteRepository(IGSCContext context, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
        }

        public List<ProjectDesignTemplateNoteGridDto> GetProjectDesignTemplateNoteList(int templateId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == templateId).
                   ProjectTo<ProjectDesignTemplateNoteGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

    }
}