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
    public class ProjectDesignTemplateRepository : GenericRespository<ProjectDesignTemplate, GscContext>,
        IProjectDesignTemplateRepository
    {
        public ProjectDesignTemplateRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser) : base(
            uow, jwtTokenAccesser)
        {
        }

        public ProjectDesignTemplate GetTemplate(int id)
        {
            var template = Context.ProjectDesignTemplate.Where(t => t.Id == id)
                .Include(d => d.VariableTemplate).ThenInclude(t => t.Notes)
                .Include(d => d.VariableTemplate).ThenInclude(t => t.VariableTemplateDetails)
                .Include(d => d.Domain)
                .Include(d => d.Variables)
                .ThenInclude(d => d.Values)
                .Include(d => d.Variables)
                .ThenInclude(d => d.Unit)
                .Include(d => d.Variables)
                .ThenInclude(d => d.VariableCategory)
                .Include(d => d.ProjectDesignVisit)
                .AsNoTracking().FirstOrDefault();

            if (template != null)
            {
                template.VariableTemplate.Notes =
                    template.VariableTemplate.Notes.Where(t => t.DeletedDate == null).ToList();
                template.Variables = template.Variables.Where(t => t.DeletedDate == null).OrderBy(t => t.DesignOrder)
                    .ToList();
                template.VariableTemplate.VariableTemplateDetails = template.VariableTemplate.VariableTemplateDetails
                    .Where(t => t.DeletedDate == null).ToList();
                template.Variables.ForEach(t =>
                {
                    t.VariableCategoryName = t.VariableCategoryId == null ? "" : t.VariableCategory.CategoryName;
                    t.Values = t.Values.Where(x => x.DeletedDate == null).OrderBy(o => o.SeqNo).ToList();
                });

                return template;
            }

            return null;
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
                screeninglockAudit = Context.ScreeningTemplateLockUnlockAudit.Where(x => x.ProjectId == lockUnlockDDDto.ChildProjectId).ToList();
            }
            else
            {
                screeninglockAudit = Context.ScreeningTemplateLockUnlockAudit.Where(x => x.ProjectId == lockUnlockDDDto.ProjectId).ToList();
            }

            if (lockUnlockDDDto.IsLock)
            {               
                var grplockedIn = screeninglockAudit.GroupBy(x => new { x.ScreeningEntryId, x.ProjectDesignId, x.ProjectDesignTemplateId })
                          .Select(y => new LockUnlockListDto()
                          {
                              Id = y.Key.ScreeningEntryId,                              
                              ProjectDesignId = y.Key.ProjectDesignId,
                              TemplateId = y.Key.ProjectDesignTemplateId,
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

                if(lockUnlockDDDto.SubjectIds != null && lockUnlockDDDto.SubjectIds.Length == 1)
                templates.RemoveAll(r => grplockedIn.Where(x => screeningEntryId.Any(y => y.Id == x.Id)).Any(a => a.TemplateId == r.Id && a.IsLocked));

            }
            else
            {
                templates = (from template in Context.ProjectDesignTemplate.Where(x => x.DeletedDate == null && x.ProjectDesignVisitId == lockUnlockDDDto.Id)
                          join locktemplate in screeninglockAudit.GroupBy(x => new { x.ScreeningEntryId, x.ProjectDesignId, x.ProjectDesignTemplateId})
                          .Select(y => new LockUnlockListDto
                          {
                              Id = y.LastOrDefault().Id,                              
                              screeningEntryId = y.Key.ScreeningEntryId,
                              ProjectDesignId = y.Key.ProjectDesignId,
                              TemplateId = y.Key.ProjectDesignTemplateId,
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

        public IList<DropDownDto> GetTemplateDropDownForProjectSchedule(int projectDesignVisitId)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignVisitId == projectDesignVisitId
                                           && x.Variables.Where(y =>
                                               y.CollectionSource == CollectionSources.Date ||
                                               y.CollectionSource == CollectionSources.Time ||
                                               y.CollectionSource == CollectionSources.DateTime).Any()).OrderBy(t => t.Id)
                .Select(t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.TemplateName,
                    Code = Context.ProjectScheduleTemplate.Any(x => x.ProjectDesignTemplateId == t.Id) ? "Used" : ""                    
                }).ToList();

            return templates;
        }

        public IList<DropDownDto> GetClonnedTemplates(int id)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ParentId == id).OrderBy(t => t.Id).Select(t => new DropDownDto
            {
                Id = t.Id,
                Value = t.TemplateName
            }).ToList();

            return templates;
        }

        public IList<ProjectDesignTemplate> GetTemplateIdsByPeriordId(int projectDesignPeriodId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignVisit.DeletedDate == null
                                                        && x.ProjectDesignVisit.ProjectDesignPeriodId ==
                                                        projectDesignPeriodId).ToList();
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

        public IList<ProjectDesignTemplate> GetAllTemplate(int projectId, int? periodId)
        {
            var parentProjectId = Context.Project.Where(x => x.Id == projectId).FirstOrDefault().ParentProjectId != null ? Context.Project.Where(x => x.Id == projectId).FirstOrDefault().ParentProjectId : projectId;

            var template = Context.ProjectDesignTemplate                
                .Include(d => d.ProjectDesignVisit)
                .ThenInclude(d => d.ProjectDesignPeriod)
                .ThenInclude(d => d.ProjectDesign).Where(d => d.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == parentProjectId && (periodId == null || d.ProjectDesignVisit.ProjectDesignPeriodId == periodId))
                .AsNoTracking().ToList();            

            return template;
        }
    }
}