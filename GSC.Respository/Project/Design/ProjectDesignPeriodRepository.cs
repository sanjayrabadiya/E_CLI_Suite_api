using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignPeriodRepository : GenericRespository<ProjectDesignPeriod, GscContext>,
        IProjectDesignPeriodRepository
    {
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        public ProjectDesignPeriodRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignVisitRepository projectDesignVisitRepository) : base(
            uow, jwtTokenAccesser)
        {
            _projectDesignVisitRepository = projectDesignVisitRepository;
        }

        public ProjectDesignPeriod GetPeriod(int id)
        {
            var period = Context.ProjectDesignPeriod.Where(t => t.Id == id)
                .Include(d => d.VisitList)
                .ThenInclude(d => d.Templates)
                .ThenInclude(d => d.Variables)
                .ThenInclude(d => d.Values)
                .AsNoTracking().FirstOrDefault();

            return period;
        }

        public IList<DropDownDto> GetPeriodDropDown(int projectDesignId)
        {
            var periods = All.Where(x => x.DeletedDate == null
                                         && x.ProjectDesignId == projectDesignId).OrderBy(t => t.Id).Select(t =>
                new DropDownDto
                {
                    Id = t.Id,
                    Value = t.DisplayName
                }).ToList();

            return periods;
        }

        public IList<DropDownWithSeqDto> GetPeriodByProjectIdDropDown(int projectId)
        {
            var periods = All.Where(x => x.DeletedDate == null && x.ProjectDesign.DeletedDate == null
                                                               && x.ProjectDesign.ProjectId == projectId &&
                                                               x.ProjectDesign.IsCompleteDesign).OrderBy(t => t.Id)
                .Select(t => new DropDownWithSeqDto
                {
                    Id = t.Id,
                    Value = t.DisplayName
                }).ToList();

            periods = periods.Select((o, i) =>
            {
                o.SeqNo = ++i;
                return o;
            }).ToList();

            return periods.OrderBy(x => x.Value).ToList();
        }

        public IList<DropDownWithSeqDto> getPeriodByProjectIdIsLockedDropDown(LockUnlockDDDto lockUnlockDDDto)
        {
            var periods = new List<DropDownWithSeqDto>();
            var screeningEntryId = Context.ScreeningEntry.Where(x => lockUnlockDDDto.SubjectIds == null || lockUnlockDDDto.SubjectIds.Contains(x.AttendanceId)).ToList();
            var screeninglockAudit = new List<ScreeningTemplateLockUnlockAudit>();
            if (lockUnlockDDDto.ChildProjectId != lockUnlockDDDto.ProjectId)
            {
                screeninglockAudit = Context.ScreeningTemplateLockUnlockAudit.Include(t => t.ScreeningTemplate).Where(x => x.ProjectId == lockUnlockDDDto.ChildProjectId).ToList();
            }
            else
            {
                screeninglockAudit = Context.ScreeningTemplateLockUnlockAudit.Include(t => t.ScreeningTemplate).Where(x => x.ProjectId == lockUnlockDDDto.ProjectId).ToList();
            }
            if (lockUnlockDDDto.IsLock)
            {
                periods = All.Where(x => x.DeletedDate == null && x.ProjectDesign.DeletedDate == null
                                                                   && x.ProjectDesign.ProjectId == lockUnlockDDDto.ProjectId &&
                                                                   x.ProjectDesign.IsCompleteDesign).OrderBy(t => t.Id)
                    .Select(t => new DropDownWithSeqDto
                    {
                        Id = t.Id,
                        Value = t.DisplayName
                    }).OrderBy(x => x.Value).ToList();

                var lstperiod = All.Where(x => x.DeletedDate == null && x.ProjectDesign.DeletedDate == null
                                                                   && x.ProjectDesign.ProjectId == lockUnlockDDDto.ProjectId &&
                                                                   x.ProjectDesign.IsCompleteDesign).OrderBy(t => t.Id).ToList();

                foreach (var item in lstperiod)
                {
                    lockUnlockDDDto.Id = item.Id;
                    var visit = _projectDesignVisitRepository.GetVisitByLockedDropDown(lockUnlockDDDto);
                    if (visit.Count == 0)
                    {
                        periods.RemoveAll(x => x.Id == item.Id);
                    }
                }
            }
            else
            {
                periods = (from design in Context.ProjectDesignPeriod.Where(x => x.DeletedDate == null && x.ProjectDesign.DeletedDate == null
                                                                     && x.ProjectDesign.ProjectId == lockUnlockDDDto.ProjectId && x.ProjectDesign.IsCompleteDesign)
                           join visit in Context.ProjectDesignVisit.Where(x => x.DeletedDate == null) on design.Id equals visit.ProjectDesignPeriodId
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
                           group design by design.Id into gcs
                           select new DropDownWithSeqDto
                           {
                               Id = gcs.Key,
                               Value = gcs.FirstOrDefault().DisplayName
                           }).Distinct().ToList();

            }
            return periods;
        }
    }
}