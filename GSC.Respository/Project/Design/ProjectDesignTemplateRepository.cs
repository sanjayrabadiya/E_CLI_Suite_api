using System.Collections;
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
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignTemplateRepository : GenericRespository<ProjectDesignTemplate>,
        IProjectDesignTemplateRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ProjectDesignTemplateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public ProjectDesignTemplate GetTemplateClone(int id)
        {
            var template = _context.ProjectDesignTemplate.
                Where(t => t.Id == id)
                .Include(d => d.ProjectDesignTemplateNote.Where(x => x.DeletedBy == null))
                .Include(d => d.Variables.Where(x => x.DeletedBy == null).OrderBy(c => c.DesignOrder))
                .ThenInclude(d => d.Values.Where(x => x.DeletedBy == null).OrderBy(c => c.SeqNo))
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
                    TemplateName = ((_jwtTokenAccesser.Language == 1) ?
                        r.TemplateLanguage.Where(x => x.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : r.TemplateName),
                    ProjectDesignVisitName = r.ProjectDesignVisit.DisplayName,
                    ActivityName = r.ActivityName,
                    // Templatenote for multilanguage
                    Notes = r.ProjectDesignTemplateNote.Where(c => c.DeletedDate == null).Select(a => a.Note).ToList(),
                    DomainId = r.DomainId,
                    IsRepeated = r.IsRepeated,
                    DesignOrder = r.DesignOrder,
                    VariableTemplateId = r.VariableTemplateId,
                    DomainName = r.Domain.DomainName
                }
            ).FirstOrDefault();

            if (result != null)
            {
                result.Variables = _context.ProjectDesignVariable.Where(t => t.ProjectDesignTemplateId == id && t.DeletedDate == null)
                    .ProjectTo<DesignScreeningVariableDto>(_mapper.ConfigurationProvider).ToList().OrderBy(r => r.DesignOrder).ToList();

                result.Variables.ToList().ForEach(x =>
                {
                    // For Variable multilanguage
                    x.VariableName = ((_jwtTokenAccesser.Language != null && _jwtTokenAccesser.Language != 1) ?
                x.VariableLanguage.Where(x => x.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : x.VariableName);

                    // For VariableNote multilanguage
                    if (x.Note != null)
                        x.Note = ((_jwtTokenAccesser.Language != null && _jwtTokenAccesser.Language != 1) ?
                   x.VariableNoteLanguage.Where(x => x.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : x.Note);

                    x.Values = x.Values.OrderBy(c => c.SeqNo).ToList();

                    // For VariableValue multilanguage
                    //if (x.Values.Count > 0)
                    //    x.Values.ToList().ForEach(r => {
                    //    r.ValueName = (_jwtTokenAccesser.Language != null ?
                    //    r.VariableValueLanguage.Where(x => x.LanguageId == (int)_jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : r.ValueName);
                    //});
                });
            }
            result = ValidateVariables(result);
            return result;
        }

        private DesignScreeningTemplateDto ValidateVariables(DesignScreeningTemplateDto designScreeningTemplateDto)
        {
            designScreeningTemplateDto.Variables.ToList().ForEach(x =>
            {
                if (x.ValidationType == ValidationType.Required)
                {
                    x.ValidationMessage = "This field is required";
                }
                else x.ValidationMessage = "";
            });
            return designScreeningTemplateDto;
        }

        public IList<DropDownDto> GetTemplateDropDown(int projectDesignVisitId)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignVisitId == projectDesignVisitId).OrderBy(t => t.Id).Select(
                t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.TemplateName,
                    Code = _context.ProjectScheduleTemplate.Any(x => x.ProjectDesignTemplateId == t.Id) ? "Used" : ""
                }).ToList();

            return templates;
        }


        public IList<DropDownDto> GetTemplateDropDownForProjectSchedule(int projectDesignVisitId, int? collectionSource, int? refVariable)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignVisitId == projectDesignVisitId
                                           && x.Variables.Where(y => collectionSource.Value > 0 ? (int)y.CollectionSource == collectionSource :
                                               y.CollectionSource == CollectionSources.Date ||
                                               y.CollectionSource == CollectionSources.Time ||
                                               y.CollectionSource == CollectionSources.DateTime).Any()
                                               // && (refVariable.Value > 0 ? !x.Variables.Any(v => _context.ProjectScheduleTemplate.Where(p => p.DeletedDate == null).Any(s => s.ProjectDesignVariableId == v.Id)) : true)
                                               && x.Variables != null
                                               ).OrderBy(t => t.Id)
                .Select(t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.TemplateName,
                    Code = _context.ProjectScheduleTemplate.Any(x => x.ProjectDesignTemplateId == t.Id) ? "Used" : "",
                    ExtraData = t.Variables.Where(y => y.Id != refVariable
                                               && !_context.ProjectScheduleTemplate.Where(p => p.DeletedDate == null).Select(x => x.ProjectDesignVariableId).Contains(y.Id)
                                               && (collectionSource.Value > 0 ? (int)y.CollectionSource == collectionSource :
                                               y.CollectionSource == CollectionSources.Date ||
                                               y.CollectionSource == CollectionSources.Time ||
                                               y.CollectionSource == CollectionSources.DateTime)
                                               ).ToList()
                }).ToList();

            return templates.ToList();
        }


        public IList<DropDownDto> GetClonnedTemplateDropDown(int id)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ParentId == id).OrderBy(t => t.Id).Select(t => new DropDownDto
                                           {
                                               Id = t.Id,
                                               Value = t.TemplateName + " - " + t.ProjectDesignVisit.DisplayName,
                                           }).ToList();

            return templates;
        }


        public IList<DropDownDto> GetTemplateDropDownByPeriodId(int projectDesignPeriodId,
            VariableCategoryType variableCategoryType)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignVisit.DeletedDate == null
                                           && x.ProjectDesignVisit.ProjectDesignPeriod.Id == projectDesignPeriodId
                                           && _context.ProjectDesignVariable.Any(t =>
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

        // Not use any where please check and remove if not use any where comment by vipul
        public IList<DropDownDto> GetTemplateDropDownAnnotation(int projectDesignVisitId)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignVisitId == projectDesignVisitId).OrderBy(t => t.Id).Select(
                t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.Domain.DomainName,
                    Code = _context.ProjectScheduleTemplate.Any(x => x.ProjectDesignTemplateId == t.Id) ? "Used" : ""
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