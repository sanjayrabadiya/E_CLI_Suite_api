using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class ManageSiteRepository : GenericRespository<ManageSite>, IManageSiteRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public ManageSiteRepository(IGSCContext context,
        IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
        : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<ManageSiteGridDto> GetManageSites(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).Include(x => x.ManageSiteAddress)
            .ProjectTo<ManageSiteGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public IList<ManageSiteDto> GetManageSiteList(int Id)
        {
            return FindByInclude(t => t.Id == Id && t.DeletedBy == null).Select(c =>
                new ManageSiteDto
                {
                    Id = c.Id,
                    SiteName = c.SiteName,
                    ContactName = c.ContactName,
                    SiteEmail = c.SiteEmail,
                    ContactNumber = c.ContactNumber,
                    SiteAddress = c.SiteAddress,
                    Status = c.Status,
                    CompanyId = c.CompanyId
                }).OrderByDescending(t => t.Id).ToList();
        }

        public string Duplicate(ManageSite objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.SiteAddress == objSave.SiteAddress.Trim() && x.DeletedDate == null))
                return "Duplicate Site Address: " + objSave.SiteAddress;

            return "";
        }
        public List<DropDownDto> GetManageSiteDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.Status)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.SiteName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
        public void UpdateRole(ManageSite ManageSite)
        {
            var siterole = _context.ManageSiteRole.Where(x => x.ManageSiteId == ManageSite.Id
                                                               && ManageSite.ManageSiteRole.Select(x => x.TrialTypeId).Contains(x.TrialTypeId)
                                                               && x.DeletedDate == null).ToList();

            ManageSite.ManageSiteRole.ForEach(z =>
            {
                var role = siterole.Find(x => x.ManageSiteId == z.ManageSiteId && x.TrialTypeId == z.TrialTypeId);
                if (role == null)
                {
                    _context.ManageSiteRole.Add(z);
                }
            });

            var managesiteRole = _context.ManageSiteRole.Where(x => x.ManageSiteId == ManageSite.Id && x.DeletedDate == null)
                .ToList();

            managesiteRole.ForEach(t =>
            {
                var role = siterole.Find(x => x.ManageSiteId == t.ManageSiteId && x.TrialTypeId == t.TrialTypeId);
                if (role == null)
                {
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.ManageSiteRole.Update(t);
                }
            });
        }

        public void UpdateSiteAddress(ManageSite objSave)
        {
            foreach (var item in objSave.ManageSiteAddress)
            {
                var originalAddress = _context.ManageSiteAddress.Find(item.Id);
                if (originalAddress != null && originalAddress.SiteAddress != item.SiteAddress)
                {
                    _context.ManageSiteAddress.Update(item);
                }

                if (item.DeletedDate != null)
                {
                    item.DeletedDate = DateTime.Now;
                    item.DeletedBy = _jwtTokenAccesser.UserId;
                    _context.ManageSiteAddress.Update(item);
                }

                if (item.Id == 0 && originalAddress == null)
                {
                    _context.ManageSiteAddress.Add(item);
                }
            }
        }


        public List<ExperienceModel> GetExperienceDetails(ExperienceFillter experienceFillter)
        {
            var experiences = _context.Project
                .Where(p => p.DeletedDate == null && p.ParentProjectId == null && _context.Users.Any(u => u.Id == p.CreatedBy))
                .Include(p => p.DesignTrial)
                .Include(p => p.Drug)
                .Include(p => p.RegulatoryType)
                .Include(p => p.DesignTrial.TrialType)
                .Include(p => p.Client)
                .Where(p => (experienceFillter.DesignTrialId == null || p.DesignTrialId == experienceFillter.DesignTrialId)
                         && (experienceFillter.TrialTypeId == null || _context.DesignTrial
                                .Where(dt => dt.TrialTypeId == experienceFillter.TrialTypeId && dt.DeletedDate == null)
                                .Select(dt => dt.Id)
                                .Contains(p.DesignTrialId))
                         && (experienceFillter.RegulatoryId == null || p.RegulatoryTypeId == experienceFillter.RegulatoryId)
                         && (experienceFillter.ClientId == null || p.ClientId == experienceFillter.ClientId)
                         && (experienceFillter.DrugId == null || p.DrugId == experienceFillter.DrugId))
                .Select(p => new
                {
                    Project = p,
                    ChildProject = _context.Project.FirstOrDefault(cp => cp.ParentProjectId == p.Id && cp.DeletedDate == null),
                    ManageSite = _context.ManageSite.FirstOrDefault(ms => ms.Id == _context.Project.FirstOrDefault(cp => cp.ParentProjectId == p.Id && cp.DeletedDate == null).ManageSiteId && ms.DeletedDate == null),
                    Investigator = _context.InvestigatorContact.FirstOrDefault(ic => ic.Id == _context.Site.FirstOrDefault(s => s.ManageSiteId == _context.ManageSite.FirstOrDefault(ms => ms.Id == _context.Project.FirstOrDefault(cp => cp.ParentProjectId == p.Id && cp.DeletedDate == null).ManageSiteId && ms.DeletedDate == null).Id && s.DeletedDate == null).InvestigatorContactId && ic.DeletedDate == null),
                    StudyPlan = _context.StudyPlan.FirstOrDefault(sp => sp.ProjectId == p.Id && sp.DeletedDate == null),
                    ProjectStatus = _context.ProjectStatus.FirstOrDefault(ps => ps.ProjectId == p.Id && ps.DeletedDate == null)
                })
                .ToList();

            var filteredData = experiences.Where(e => (experienceFillter.StartDate == null || e.StudyPlan?.StartDate > experienceFillter.StartDate)
                                                   && (experienceFillter.EndDate == null || e.StudyPlan?.EndDate < experienceFillter.EndDate)
                                                   && (experienceFillter.InvestigatorId == null || e.Investigator?.Id == experienceFillter.InvestigatorId)
                                                   && (experienceFillter.ProjectStatusId == null || e.ProjectStatus?.Status == experienceFillter.ProjectStatusId));

            var groupedData = filteredData.GroupBy(e => e.Project.Id)
                .Select(g => new ExperienceModel
                {
                    ProjectId = g.Key,
                    StudyName = g.First().Project.ProjectName,
                    InvestigatorNames = g.Select(e => e.Investigator?.NameOfInvestigator).Distinct().ToList(),
                    StartDate = g.First().StudyPlan?.StartDate,
                    EndDate = g.First().StudyPlan?.EndDate,
                    ProjectStatus = g.First().ProjectStatus?.Status.GetDescription(),
                    NoOfSite = g.Select(e => e.ChildProject?.Id).Distinct().Count(),
                    TargetedSubjects = g.Select(e => e.ChildProject?.AttendanceLimit).Distinct().ToList(),
                    SiteNames = g.Select(e => e.ManageSite?.SiteName).Distinct().ToList(),
                    NumberOfPatients = g.First().Project.AttendanceLimit,
                    DrugName = g.First().Project.Drug.DrugName,
                    Submission = g.First().Project.RegulatoryType.RegulatoryTypeName,
                    TypeOfTrial = g.First().Project.DesignTrial.DesignTrialName,
                    TherapeuticIndication = g.First().Project.DesignTrial.TrialType.TrialTypeName,
                    NoOfCountry = g.Select(e => e.ChildProject?.CountryId).Distinct().Count(),
                    StudyCode = g.First().Project.ProjectCode,
                    ClientName = g.First().Project.Client?.ClientName
                })
                .ToList();

            return groupedData;
        }
    }
}
