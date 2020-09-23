using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignVisitStatusRepository : GenericRespository<ProjectDesignVisitStatus, GscContext>, IProjectDesignVisitStatusRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public ProjectDesignVisitStatusRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(uow,
            jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        //added by vipul for get visit status by visit id on 23092020

        public ProjectDesignVisitStatusDto GetProjectDesignVisitStatusByVisitId(int VisitId)
        {
            return All.Where(x=> x.DeletedDate == null && x.ProjectDesignVisitId == VisitId).Select(t => new ProjectDesignVisitStatusDto
            {
                 Id = t.Id,
                 ProjectDesignVisitId = t.ProjectDesignVisitId,
                ProjectDesignTemplateId= t.ProjectDesignVariable.ProjectDesignTemplateId,
                ProjectDesignVariableId = t.ProjectDesignVariableId,
                VisitStatusId = t.VisitStatusId
             }).FirstOrDefault();
        }
    }
}