using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Rights;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Linq;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Rights
{
    public class ProjectModuleRightsRepository : GenericRespository<GSC.Data.Entities.Project.Rights.ProjectModuleRights>, IProjectModuleRightsRepository
    {
        private readonly IGSCContext _context;

        public ProjectModuleRightsRepository(IGSCContext context)
         : base(context)
        {
            _context = context;
        }

        public void Save(StudyModuleDto details)
        {
            var project = _context.Project.Where(t => t.ProjectCode == details.StudyCode).SingleOrDefault();

            var existing = _context.ProjectModuleRights.Where(t => t.ProjectID == project.Id).ToList();
            if (existing.Any())
            {
                _context.ProjectModuleRights.RemoveRange(existing);
                _context.Save();
            }
            List<GSC.Data.Entities.Project.Rights.ProjectModuleRights> rightdetails = new List<Data.Entities.Project.Rights.ProjectModuleRights>();
            var appscreen = _context.AppScreen.ToList();
            foreach (var module in details.StudyModules)
            {
                GSC.Data.Entities.Project.Rights.ProjectModuleRights data = new Data.Entities.Project.Rights.ProjectModuleRights();
                data.ProjectID = project.Id;
                data.AppScreenID = appscreen.Where(a => a.ScreenCode == module.ModuleCode).Select(i => i.Id).SingleOrDefault();
                rightdetails.Add(data);
            }
            _context.ProjectModuleRights.AddRange(rightdetails);
            _context.Save();
        }

    }
}
