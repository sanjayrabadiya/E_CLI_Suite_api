using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignVisitRepository : GenericRespository<ProjectDesignVisit, GscContext>,
        IProjectDesignVisitRepository
    {
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        public ProjectDesignVisitRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignTemplateRepository projectDesignTemplateRepository) : base(uow,
            jwtTokenAccesser)
        {
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
        }

        public ProjectDesignVisit GetVisit(int id)
        {
            var visit = Context.ProjectDesignVisit.Where(t => t.Id == id)
                .Include(d => d.Templates)
                .ThenInclude(d => d.Variables)
                .ThenInclude(d => d.Values)
                .AsNoTracking().FirstOrDefault();

            return visit;
        }

        public IList<DropDownDto> GetVisitsByProjectDesignId(int projectDesignId)
        {
            var periods = Context.ProjectDesignPeriod.Where(x => x.DeletedDate == null
                                                                 && x.ProjectDesignId == projectDesignId)
                .Include(t => t.VisitList);

            var visits = new List<DropDownDto>();
            periods.ForEach(period =>
            {
                period.VisitList.Where(x => x.DeletedDate == null)
                    .ForEach(visit =>
                    {
                        visits.Add(new DropDownDto
                        {
                            Id = visit.Id,
                            Value = visit.DisplayName + " (" + period.DisplayName + ")"
                        });
                    });
            });

            return visits;
        }

        public IList<DropDownDto> GetVisitDropDown(int projectDesignPeriodId)
        {
            var visits = All.Where(x => x.DeletedDate == null
                                        && x.ProjectDesignPeriodId == projectDesignPeriodId).OrderBy(t => t.Id).Select(
                t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.DisplayName
                }).ToList();

            return visits;
        }

        public IList<DropDownDto> GetVisitByLockedDropDown(LockUnlockDDDto lockUnlockDDDto)
        {
            var visits = new List<DropDownDto>();
            var screeningEntryId = Context.ScreeningEntry.Where(x => lockUnlockDDDto.SubjectIds == null || lockUnlockDDDto.SubjectIds.Contains(x.AttendanceId)).ToList();
            var screeninglockAudit = new List<ScreeningTemplateLockUnlockAudit>();
            if (lockUnlockDDDto.ChildProjectId != lockUnlockDDDto.ProjectId)
                screeninglockAudit = Context.ScreeningTemplateLockUnlockAudit.Include(t => t.ScreeningTemplate).Where(x => x.ProjectId == lockUnlockDDDto.ChildProjectId).ToList();
            else
                screeninglockAudit = Context.ScreeningTemplateLockUnlockAudit.Include(t => t.ScreeningTemplate).Where(x => x.ProjectId == lockUnlockDDDto.ProjectId).ToList();

            if (lockUnlockDDDto.IsLock)
            {
                var lstvisit = Context.ProjectDesignVisit.Where(x => x.DeletedDate == null
                                            && x.ProjectDesignPeriodId == lockUnlockDDDto.Id).Include(x => x.Templates).OrderBy(t => t.Id).ToList();

                visits = All.Where(x => x.DeletedDate == null
                                            && x.ProjectDesignPeriodId == lockUnlockDDDto.Id).OrderBy(t => t.Id).Select(
                    t => new DropDownDto
                    {
                        Id = t.Id,
                        Value = t.DisplayName
                    }).ToList();

                foreach (var item in lstvisit)
                {
                    lockUnlockDDDto.Id = item.Id;
                    var template = _projectDesignTemplateRepository.GetTemplateByLockedDropDown(lockUnlockDDDto);
                    if (template.Count == 0)
                    {
                        visits.RemoveAll(x => x.Id == item.Id);
                    }
                }

            }
            else
            {
                visits = (from visit in Context.ProjectDesignVisit.Where(x => x.DeletedDate == null && x.ProjectDesignPeriodId == lockUnlockDDDto.Id)
                          join template in Context.ProjectDesignTemplate.Where(x => x.DeletedDate == null) on visit.Id equals template.ProjectDesignVisitId
                          join locktemplate in screeninglockAudit.GroupBy(x => new { x.ScreeningEntryId, x.ScreeningTemplateId })
                          .Select(y => new LockUnlockListDto
                          {
                              Id = y.LastOrDefault().Id,
                              screeningEntryId = y.Key.ScreeningEntryId,
                              ScreeningTemplateId = y.Key.ScreeningTemplateId,
                              TemplateId = y.LastOrDefault(t => t.ScreeningTemplateId == y.Key.ScreeningTemplateId).ScreeningTemplate.ProjectDesignTemplateId,
                              IsLocked = y.LastOrDefault().IsLocked
                          }).Where(x => x.IsLocked && screeningEntryId.Any(y => y.Id == x.screeningEntryId)).ToList()
                          on template.Id equals locktemplate.TemplateId
                          group visit by visit.Id into gcs
                          select new DropDownDto
                          {
                              Id = gcs.Key,
                              Value = gcs.FirstOrDefault().DisplayName,
                          }).OrderBy(t => t.Id).Distinct().ToList();
            }
            return visits;
        }

        public IList<ProjectDesignVisitBasicDto> GetVisitAndTemplateByPeriordId(int projectDesignPeriodId)
        {
            return All.Where(x => x.DeletedDate == null && x.DeletedDate == null && x.ProjectDesignPeriodId == projectDesignPeriodId)
                .Select(t => new ProjectDesignVisitBasicDto
                {
                    Id = t.Id,
                    IsRepeated = t.IsRepeated,
                    IsSchedule = t.IsSchedule,
                    Templates = t.Templates.Where(a => a.DeletedDate == null).Select(b => b.Id).ToList()
                }).ToList();
        }

        public string Duplicate(ProjectDesignVisit objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.DisplayName == objSave.DisplayName &&
                x.ProjectDesignPeriodId == objSave.ProjectDesignPeriodId && x.DeletedDate == null))
                return "Duplicate Visit Name : " + objSave.DisplayName;
            return "";
        }
    }
}