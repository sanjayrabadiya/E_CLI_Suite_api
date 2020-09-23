using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public class ProjectDesignTemplateRepository : GenericRespository<ProjectDesignTemplate, GscContext>,
        IProjectDesignTemplateRepository
    {
        private readonly IMapper _mapper;
        public ProjectDesignTemplateRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(
            uow, jwtTokenAccesser)
        {
            _mapper = mapper;
        }

        public ProjectDesignTemplate GetTemplateClone(int id)
        {
            var template = Context.ProjectDesignTemplate.
                Where(t => t.Id == id)
                .Include(d => d.Variables)
                .ThenInclude(d => d.Values)
                .AsNoTracking().FirstOrDefault();

            return template;


        }


        public DesignScreeningTemplateDto GetTemplate(int id)
        {
            var result = All.Where(t => t.Id == id).
                Select(r => new DesignScreeningTemplateDto
                {
                    Id = r.Id,
                    ProjectDesignTemplateId = r.Id,
                    ProjectDesignVisitId = r.ProjectDesignVisitId,
                    TemplateName = r.TemplateName,
                    ProjectDesignVisitName = r.ProjectDesignVisit.DisplayName,
                    ActivityName = r.ActivityName,
                    Variables = null,
                    Notes = Context.VariableTemplateNote.
                    Where(c => c.DeletedDate == null && c.VariableTemplateId == r.VariableTemplateId).Select(a => a.Note).ToList(),
                    DomainId = r.DomainId,
                    IsRepeated = r.IsRepeated,
                    DesignOrder = r.DesignOrder,
                    VariableTemplateId = r.VariableTemplateId,
                    DomainName = r.Domain.DomainName
                }
            ).FirstOrDefault();

            if (result != null)
            {
                result.Variables = Context.ProjectDesignVariable.Where(t => t.ProjectDesignTemplateId == id && t.DeletedDate == null)
                    .ProjectTo<DesignScreeningVariableDto>(_mapper.ConfigurationProvider).ToList().OrderBy(r => r.DesignOrder).ToList();

                var variableNotes = Context.VariableTemplateDetail.Where(x => x.VariableTemplateId == result.VariableTemplateId).ToList();
                result.Variables.ForEach(x =>
                {
                    x.Note = variableNotes.FirstOrDefault(c => c.VariableId == x.VariableId)?.Note;
                });
            }

            return result;
        }

        public IList<DropDownDto> GetTemplateDropDown(int projectDesignVisitId)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignVisitId == projectDesignVisitId).OrderBy(t => t.Id).Select(
                t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.TemplateName,
                    Code = Context.ProjectScheduleTemplate.Any(x => x.ProjectDesignTemplateId == t.Id) ? "Used" : ""
                }).ToList();

            return templates;
        }

        public IList<DropDownDto> GetTemplateByLockedDropDown(LockUnlockDDDto lockUnlockDDDto)
        {
            var templates = new List<DropDownDto>();
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
                var grplockedIn = screeninglockAudit.GroupBy(x => new { x.ScreeningEntryId, x.ScreeningTemplateId })
                          .Select(y => new LockUnlockListDto()
                          {
                              Id = y.Key.ScreeningEntryId,
                              ScreeningTemplateId = y.Key.ScreeningTemplateId,
                              TemplateId = y.LastOrDefault(t => t.ScreeningTemplateId == y.Key.ScreeningTemplateId).ScreeningTemplate.ProjectDesignTemplateId,
                              IsLocked = y.LastOrDefault().IsLocked,
                              ProjectId = y.LastOrDefault().ProjectId
                          }).ToList();

                templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignVisitId == lockUnlockDDDto.Id).OrderBy(t => t.Id).Select(
                t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.TemplateName,
                    Code = Context.ProjectScheduleTemplate.Any(x => x.ProjectDesignTemplateId == t.Id) ? "Used" : ""
                }).ToList();

                if (lockUnlockDDDto.SubjectIds != null && lockUnlockDDDto.SubjectIds.Length == 1)
                    templates.RemoveAll(r => grplockedIn.Where(x => screeningEntryId.Any(y => y.Id == x.Id)).Any(a => a.TemplateId == r.Id && a.IsLocked));

            }
            else
            {
                templates = (from template in Context.ProjectDesignTemplate.Where(x => x.DeletedDate == null && x.ProjectDesignVisitId == lockUnlockDDDto.Id)
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
                             group template by template.Id into gcs
                             select new DropDownDto
                             {
                                 Id = gcs.Key,
                                 Value = gcs.FirstOrDefault().TemplateName
                             }).Distinct().ToList();
            }

            return templates;
        }

        public IList<DropDownDto> GetTemplateDropDownForProjectSchedule(int projectDesignVisitId, int? collectionSource, int? refVariable)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignVisitId == projectDesignVisitId
                                           && !x.Variables.Any(c => c.Id == refVariable)
                                           && x.Variables.Where(y => collectionSource.Value > 0 ? (int)y.CollectionSource == collectionSource :
                                               y.CollectionSource == CollectionSources.Date ||
                                               y.CollectionSource == CollectionSources.Time ||
                                               y.CollectionSource == CollectionSources.DateTime).Any()
                                               && (refVariable.Value > 0 ? !x.Variables.Any(v => Context.ProjectScheduleTemplate.Where(p => p.DeletedDate == null).Any(s => s.ProjectDesignVariableId == v.Id)) : true)
                                               && x.Variables != null).OrderBy(t => t.Id)
                .Select(t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.TemplateName,
                    Code = Context.ProjectScheduleTemplate.Any(x => x.ProjectDesignTemplateId == t.Id) ? "Used" : "",
                    ExtraData = t.Variables.Where(y => collectionSource.Value > 0 ? (int)y.CollectionSource == collectionSource :
                                               y.CollectionSource == CollectionSources.Date ||
                                               y.CollectionSource == CollectionSources.Time ||
                                               y.CollectionSource == CollectionSources.DateTime).ToList()
                }).ToList();

            return templates;
        }


        public IList<DropDownDto> GetClonnedTemplateDropDown(int id)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ParentId == id).OrderBy(t => t.Id).Select(t => new DropDownDto
                                           {
                                               Id = t.Id,
                                               Value = t.TemplateName
                                           }).ToList();

            return templates;
        }


        public IList<DropDownDto> GetTemplateDropDownByPeriodId(int projectDesignPeriodId,
            VariableCategoryType variableCategoryType)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignVisit.DeletedDate == null
                                           && x.ProjectDesignVisit.ProjectDesignPeriod.Id == projectDesignPeriodId
                                           && Context.ProjectDesignVariable.Any(t =>
                                               t.SystemType == variableCategoryType
                                               && t.DeletedDate == null
                                               && t.ProjectDesignTemplateId == x.Id)
            ).OrderBy(t => t.Id).Select(t => new DropDownDto
            {
                Id = t.Id,
                Value = t.TemplateName + " " + t.ProjectDesignVisit.DisplayName
            }).ToList();

            return templates;
        }

        public IList<DropDownDto> GetTemplateDropDownAnnotation(int projectDesignVisitId)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignVisitId == projectDesignVisitId).OrderBy(t => t.Id).Select(
                t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.Domain.DomainName,
                    Code = Context.ProjectScheduleTemplate.Any(x => x.ProjectDesignTemplateId == t.Id) ? "Used" : ""
                }).ToList();

            return templates;
        }

        //added by vipul for get only date time variable template in project design visit on 22092020
        public IList<DropDownDto> GetTemplateDropDownForVisitStatus(int projectDesignVisitId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignVisitId == projectDesignVisitId
            && x.Variables.Where(y => (y.CollectionSource == CollectionSources.Date || y.CollectionSource == CollectionSources.DateTime) && y.DeletedDate == null).Any()).OrderBy(t => t.Id)
                .Select(t => new DropDownDto { Id = t.Id, Value = t.TemplateName }).ToList();
        }
    }
}