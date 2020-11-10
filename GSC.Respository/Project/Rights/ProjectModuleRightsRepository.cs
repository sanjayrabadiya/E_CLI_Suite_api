using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Rights;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Linq;
namespace GSC.Respository.Project.Rights
{
    public class ProjectModuleRightsRepository : GenericRespository<GSC.Data.Entities.Project.Rights.ProjectModuleRights, GscContext>, IProjectModuleRightsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;

        public ProjectModuleRightsRepository(IUnitOfWork<GscContext> uow,
         IJwtTokenAccesser jwtTokenAccesser,
         IMapper mapper)
         : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _uow = uow;
        }

        public void Save(StudyModuleDto details)
        {
            var project = Context.Project.Where(t => t.ProjectCode == details.StudyCode).SingleOrDefault();
            
            var existing = Context.ProjectModuleRights.Where(t => t.ProjectID == project.Id).ToList();
            if (existing.Any())
            {
                Context.ProjectModuleRights.RemoveRange(existing);
                _uow.Save();
            }
            List<GSC.Data.Entities.Project.Rights.ProjectModuleRights> rightdetails = new List<Data.Entities.Project.Rights.ProjectModuleRights>();
            var appscreen = Context.AppScreen.ToList();
            foreach(var module in details.StudyModules)
            {
                GSC.Data.Entities.Project.Rights.ProjectModuleRights data = new Data.Entities.Project.Rights.ProjectModuleRights();
                data.ProjectID = project.Id;
                data.AppScreenID = appscreen.Where(a => a.ScreenCode == module.ModuleCode).Select(i => i.Id).SingleOrDefault();
                rightdetails.Add(data);
            }
            Context.ProjectModuleRights.AddRange(rightdetails);
            _uow.Save();
        }

    }
}
