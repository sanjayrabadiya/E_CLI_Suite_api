using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Configuration
{
    public class DashboardCompanyRepository : GenericRespository<Company>, IDashboardCompanyRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public DashboardCompanyRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }
        public List<DashboardCompanyGridDto> GetDashboardCompanyList()
        {
            var projectList = _context.Company.Include(x => x.Location).Where(x => x.DeletedDate == null)
                .Select(c => new DashboardCompanyGridDto
                {
                    Id= c.Id,
                    CompanyCode = c.CompanyCode,
                    CompanyName = c.CompanyName,
                    Phone1 = c.Phone1,
                    Phone2 = c.Phone2,
                    CountryName = c.Location.Country.CountryName,
                    Address = c.Location.Address,
                    CreatedDate = c.CreatedDate.Value
                }).ToList();
            return projectList;
        }
        public dynamic GetDashboardProjectsStatus()
        {
            var projectList = _context.ProjectDocumentReview.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.Project.ParentProjectId == null
                                       && _context.ProjectRight.Any(a => (a.project.ParentProjectId == x.ProjectId || a.ProjectId == x.ProjectId)
                                                                        && a.UserId == _jwtTokenAccesser.UserId
                                                                        && a.RoleId == _jwtTokenAccesser.RoleId
                                                                        && a.DeletedDate == null
                                                                        && a.RollbackReason == null)
                                       && x.DeletedDate == null)
          .Select(c => new DashboardProject
          {
              ProjectId = c.ProjectId,
              CreatedDate = c.CreatedDate.Value
          }).ToList();

            var childParentList = _context.ProjectDocumentReview.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.Project.ParentProjectId != null
                               && _context.ProjectRight.Any(a => (a.project.ParentProjectId == x.ProjectId || a.ProjectId == x.ProjectId)
                                                                && a.UserId == _jwtTokenAccesser.UserId
                                                                && a.RoleId == _jwtTokenAccesser.RoleId
                                                                && a.DeletedDate == null
                                                                && a.RollbackReason == null) &&
                               x.DeletedDate == null)
         .Select(c => new DashboardProject
         {
             ProjectId = (int)c.Project.ParentProjectId,
             CreatedDate = c.CreatedDate.Value
         }).ToList();

         projectList.AddRange(childParentList);

            var projects = projectList.GroupBy(d => d.ProjectId).Select(c => new DashboardProject
            {
                ProjectId = c.FirstOrDefault().ProjectId,
                CreatedDate = c.FirstOrDefault().CreatedDate
            }).OrderBy(o => o.ProjectId).ToList();

            //chart disply data
            var chartFinal = _context.ProjectStatus.Where(x => projects.Select(x => x.ProjectId).Contains(x.ProjectId) && x.DeletedDate == null)
                .GroupBy(x => new { x.Status}).Select(s => new
                 {
                    Status = s.Key.Status.GetDescription(),
                    Count = s.Where(b => b.Status == s.Key.Status).Count()
                 }).ToList();

            //Grid disply data
            var gridFinal = _context.ProjectStatus.Where(x => projects.Select(x => x.ProjectId).Contains(x.ProjectId) && x.DeletedDate == null)
                .Select(c => new DashboardStudyGridDto
                {
                    projectName = _context.Project.Where(x=>x.Id==c.ProjectId && x.DeletedBy == null).Select(p=>p.ProjectCode).FirstOrDefault(),
                    Status =  c.Status.GetDescription(),
                    Country = _context.Project.Include(x=>x.Country).Where(x => x.Id == c.ProjectId && x.DeletedBy == null).Select(p => p.Country.CountryName).FirstOrDefault(),
                    CreatedDate= c.CreatedDate.Value
                }).ToList();

            dynamic[] data = new dynamic[2];
            data[0] = chartFinal;
            data[1] = gridFinal;

            return data;
        }
    }
}