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
            foreach (var item in objSave.ManageSiteAddress)
            {
                if (_context.ManageSiteAddress.Any(x => x.Id != item.Id && x.SiteAddress == item.SiteAddress.Trim() && x.DeletedDate == null))
                    return "Duplicate Site Address: " + item.SiteAddress;
            }

            return "";
        }
        public List<DropDownDto> GetManageSiteDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.Status == true)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.SiteName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
        public void UpdateRole(ManageSite ManageSite)
        {
            var siterole = _context.ManageSiteRole.Where(x => x.ManageSiteId == ManageSite.Id
                                                               && ManageSite.ManageSiteRole.Select(x => x.TrialTypeId).Contains(x.TrialTypeId)
                                                               && x.DeletedDate == null).ToList();

            ManageSite.ManageSiteRole.ForEach(z =>
            {
                var role = siterole.Where(x => x.ManageSiteId == z.ManageSiteId && x.TrialTypeId == z.TrialTypeId).FirstOrDefault();
                if (role == null)
                {
                    _context.ManageSiteRole.Add(z);
                }
            });

            var managesiteRole = _context.ManageSiteRole.Where(x => x.ManageSiteId == ManageSite.Id && x.DeletedDate == null)
                .ToList();

            managesiteRole.ForEach(t =>
            {
                var role = siterole.Where(x => x.ManageSiteId == t.ManageSiteId && x.TrialTypeId == t.TrialTypeId).FirstOrDefault();
                if (role == null)
                {
                    //delete
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
                var orginalAddress = _context.ManageSiteAddress.Find(item.Id);
                if (orginalAddress != null)
                {
                    if (orginalAddress.SiteAddress != item.SiteAddress)
                    {
                        _context.ManageSiteAddress.Update(item);
                    }
                }

                if (item.DeletedDate != null)
                {
                    item.DeletedDate = DateTime.Now;
                    item.DeletedBy = _jwtTokenAccesser.UserId;
                    _context.ManageSiteAddress.Update(item);
                }

                if (item.Id == 0 && orginalAddress == null)
                {
                    _context.ManageSiteAddress.Add(item);
                }
            }
        }

        public List<ExperienceModel> GetExperienceDetails(ExperienceFillter experienceFillter)
        {
            var experiences = new List<ExperienceModel>();
            var designIds = _context.DesignTrial.Where(x => x.TrialTypeId == experienceFillter.TrialTypeId && x.DeletedDate == null).Select(s => s.Id).ToList();

            var data = (from p in _context.Project.Where(q => q.DeletedDate == null && q.ParentProjectId == null && _context.Users.Any(x => x.Id == q.CreatedBy)).Include(x => x.DesignTrial)
                                                          .Include(x => x.Drug)
                                                          .Include(x => x.RegulatoryType)
                                                          .Include(x => x.DesignTrial.TrialType)
                                                          .Include(x => x.Client)
                                                          .Where(x => (experienceFillter.DesignTrialId != null ? x.DesignTrialId == experienceFillter.DesignTrialId : true)
                                                          && (experienceFillter.TrialTypeId != null ? designIds.Contains(x.DesignTrialId) : true)
                                                          && (experienceFillter.RegulatoryId != null ? x.RegulatoryTypeId == experienceFillter.RegulatoryId : true)
                                                          && (experienceFillter.ClientId != null ? x.ClientId == experienceFillter.ClientId : true)
                                                          && (experienceFillter.DrugId != null ? x.DrugId == experienceFillter.DrugId : true))
                        join ps in _context.ProjectStatus.Where(q => q.DeletedDate == null) on p.Id equals ps.ProjectId into p_ps
                        from subProjectStatus in p_ps.DefaultIfEmpty()
                        join cp in _context.Project.Where(q => q.DeletedDate == null && q.ParentProjectId != null) on p.Id equals cp.ParentProjectId into p_cp
                        from subChildProject in p_cp.DefaultIfEmpty()
                        join ms in _context.ManageSite.Where(q => q.DeletedDate == null) on subChildProject.ManageSiteId equals ms.Id into p_ms
                        from subMangeSite in p_ms.DefaultIfEmpty()
                        join s in _context.Site.Where(q => q.DeletedDate == null) on subMangeSite.Id equals s.ManageSiteId into p_s
                        from subSite in p_s.DefaultIfEmpty()
                        join c in _context.InvestigatorContact.Where(q => q.DeletedDate == null) on subSite.InvestigatorContactId equals c.Id into p_c
                        from subInvestiator in p_c.DefaultIfEmpty()
                        join sp in _context.StudyPlan.Where(q => q.DeletedDate == null) on p.Id equals sp.ProjectId into p_sp
                        from subStudyPlan in p_sp.DefaultIfEmpty()
                        select new
                        {
                            Project = p,
                            ChildProject = subChildProject,
                            ManageSite = subMangeSite,
                            Investigator = subInvestiator,
                            StudyPlan = subStudyPlan,
                            ProjectStatus = subProjectStatus,
                        });

            foreach (var pro in data)
            {
                var exp = new ExperienceModel();
                exp.ProjectId = pro.Project.Id;
                exp.InvestigatorId = pro.Investigator?.Id ?? 0;
                exp.SiteId = pro.ChildProject?.Id ?? 0;
                exp.DrugName = pro.Project.Drug.DrugName;
                exp.InvestigatorName = pro.Investigator?.NameOfInvestigator ?? "";
                exp.NumberOfPatients = pro.Project.AttendanceLimit;
                exp.ProjectStatus = pro.ProjectStatus?.Status.GetDescription() ?? "";
                exp.StudyName = pro.Project.ProjectName;
                exp.StudyCode = pro.Project.ProjectCode;
                exp.StudyDuration = "";
                exp.StartDate = pro.StudyPlan?.StartDate ?? null;
                exp.SiteName = pro.ManageSite?.SiteName ?? "";
                exp.EndDate = pro.StudyPlan?.EndDate ?? null;
                exp.Submission = pro.Project.RegulatoryType.RegulatoryTypeName;
                exp.TherapeuticIndication = pro.Project.DesignTrial.TrialType.TrialTypeName;
                exp.TypeOfTrial = pro.Project.DesignTrial.DesignTrialName;
                exp.TargetedSubject = pro.ChildProject?.AttendanceLimit ?? 0;
                exp.CountryId = pro.ChildProject?.CountryId ?? 0;
                exp.ProjectStatusId = pro.ProjectStatus?.Status;
                exp.ClientName = pro.Project.Client?.ClientName ?? "";
                experiences.Add(exp);
            }



            var fillterData = experiences.Where(x => (experienceFillter.StartDate != null && experienceFillter.EndDate == null) ? x.StartDate > experienceFillter.StartDate : (experienceFillter.StartDate == null && experienceFillter.EndDate != null) ? x.EndDate < experienceFillter.EndDate : (experienceFillter.StartDate != null && experienceFillter.EndDate != null) ? x.StartDate > experienceFillter.StartDate && x.EndDate < experienceFillter.EndDate : true
            && (experienceFillter.InvestigatorId != null ? x.InvestigatorId == experienceFillter.InvestigatorId : true)
            && (experienceFillter.ProjectStatusId != null ? x.ProjectStatusId == experienceFillter.ProjectStatusId : true));

            var groupData = fillterData.GroupBy(x => x.ProjectId)
                .Select(s => new ExperienceModel()
                {
                    ProjectId = s.Key,
                    StudyName = s.FirstOrDefault().StudyName,
                    InvestigatorNames = s.Select(s => s.InvestigatorName).Distinct().ToList(),
                    StartDate = s.FirstOrDefault().StartDate,
                    EndDate = s.FirstOrDefault().EndDate,
                    ProjectStatus = s.FirstOrDefault().ProjectStatus,
                    NoOfSite = s.Where(q => q.SiteId > 0).Select(s => s.SiteId).Distinct().Count(),
                    TargetedSubjects = s.Select(q => q.TargetedSubject).Distinct().ToList(),
                    SiteNames = s.Select(s => s.SiteName).Distinct().ToList(),
                    NumberOfPatients = s.FirstOrDefault().NumberOfPatients,
                    DrugName = s.FirstOrDefault().DrugName,
                    Submission = s.FirstOrDefault().Submission,
                    TypeOfTrial = s.FirstOrDefault().TypeOfTrial,
                    TherapeuticIndication = s.FirstOrDefault().TherapeuticIndication,
                    NoOfCountry = s.Where(q => q.CountryId > 0).Select(s => s.CountryId).Distinct().Count(),
                    StudyCode = s.FirstOrDefault().StudyCode,
                    ClientName = s.FirstOrDefault().ClientName
                });

            return groupData.ToList();
        }
    }
}
