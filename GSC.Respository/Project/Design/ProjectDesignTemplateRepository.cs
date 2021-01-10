using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
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
                    TemplateName = ((_jwtTokenAccesser.Language != 1) ?
                        r.TemplateLanguage.Where(x => x.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : r.TemplateName),
                    ProjectDesignVisitName = r.ProjectDesignVisit.DisplayName,
                    ActivityName = r.ActivityName,
                    Notes = _jwtTokenAccesser.Language != 1 ? r.TemplateLanguage.Where(c => c.DeletedDate == null && c.LanguageId == _jwtTokenAccesser.Language).Select(a => a.Display).ToList() : r.ProjectDesignTemplateNote.Where(c => c.DeletedDate == null).Select(a => a.Note).ToList(),
                    DomainId = r.DomainId,
                    IsRepeated = r.IsRepeated,
                    IsSchedule = r.ProjectDesignVisit.IsSchedule ?? false,
                    DesignOrder = r.DesignOrder,
                    VariableTemplateId = r.VariableTemplateId,
                    DomainName = r.Domain.DomainName
                }
            ).FirstOrDefault();

            if (result != null)
            {

                var variables = _context.ProjectDesignVariable.Where(t => t.ProjectDesignTemplateId == id && t.DeletedDate == null)
                    .Select(x => new DesignScreeningVariableDto
                    {
                        ProjectDesignTemplateId = x.ProjectDesignTemplateId,
                        ProjectDesignVariableId = x.Id,
                        Id = x.Id,
                        VariableName = (_jwtTokenAccesser.Language != 1 ?
                        x.VariableLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : x.VariableName),
                        VariableCode = x.VariableCode,
                        CollectionSource = x.CollectionSource,
                        ValidationType = x.ValidationType,
                        DataType = x.DataType,
                        Length = x.Length,
                        DefaultValue = x.DefaultValue,
                        LowRangeValue = x.LowRangeValue,
                        HighRangeValue = x.HighRangeValue,
                        PrintType = x.PrintType,
                        Values = x.Values.Where(x => x.DeletedDate == null).Select(c => new ScreeningVariableValueDto
                        {
                            Id = c.Id,
                            ProjectDesignVariableId = c.ProjectDesignVariableId,
                            ValueName = _jwtTokenAccesser.Language != 1 ? c.VariableValueLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : c.ValueName,
                            SeqNo = c.SeqNo,
                            Label = c.Label,
                        }).ToList(),
                        Remarks = _mapper.Map<List<ScreeningVariableRemarksDto>>(x.Remarks.Where(x => x.DeletedDate == null)),
                        UnitName = x.Unit.UnitName,
                        DesignOrder = x.DesignOrder,
                        IsDocument = x.IsDocument,
                        VariableCategoryName = x.VariableCategory.CategoryName ?? "",
                        SystemType = x.SystemType,
                        IsNa = x.IsNa,
                        DateValidate = x.DateValidate,
                        Note = (_jwtTokenAccesser.Language != 1 ?
                        x.VariableNoteLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : x.Note),
                        ValidationMessage = x.ValidationType == ValidationType.Required ? "This field is required" : "",
                    }).OrderBy(r => r.DesignOrder).ToList();


                variables.ForEach(x =>
                {
                    x.Values = x.Values.OrderBy(c => c.SeqNo).ToList();
                });

                result.Variables = variables;
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