﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;


namespace GSC.Respository.Project.StudyLevelFormSetup
{
    public class StudyLevelFormRepository : GenericRespository<StudyLevelForm>, IStudyLevelFormRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IStudyLevelFormVariableValueRepository _studyLevelFormVariableValueRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        public StudyLevelFormRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
            IStudyLevelFormVariableValueRepository studyLevelFormVariableValueRepository,
            IProjectRightRepository projectRightRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _studyLevelFormVariableValueRepository = studyLevelFormVariableValueRepository;
            _projectRightRepository = projectRightRepository;
        }

        public List<StudyLevelFormGridDto> GetStudyLevelFormList(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<StudyLevelFormGridDto>();

            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && projectList.Any(c => c == x.ProjectId)).
                   ProjectTo<StudyLevelFormGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(StudyLevelForm objSave)
        {
            var VariableTemplate = _context.VariableTemplate.Where(x => x.Id == objSave.VariableTemplateId).FirstOrDefault();
            if (All.Any(x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId && x.AppScreenId == objSave.AppScreenId
            && x.ActivityId == objSave.ActivityId && x.DeletedDate == null) && VariableTemplate != null)
            {
                return "Duplicate Form  : " + VariableTemplate.TemplateName;
            }

            return "";
        }

        public IList<DropDownDto> GetTemplateDropDown(int projectId)
        {
            var template = All.Where(x => x.DeletedDate == null && x.ProjectId == projectId).Select(
                t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.VariableTemplate.TemplateName,
                    Code = t.VariableTemplate.TemplateCode
                }).ToList();

            return template;
        }

        public CtmsMonitoringReportFormDto GetReportFormVariable(int id)
        {
            var result = All.Where(t => t.Id == id).Include(x => x.Activity).ThenInclude(x => x.CtmsActivity)
                .Include(x => x.VariableTemplate).ThenInclude(d => d.VariableTemplateDetails)
                .Include(x => x.VariableTemplate).ThenInclude(d => d.Notes)
                .Select(r => new CtmsMonitoringReportFormDto
                {
                    Id = r.Id,
                    VariableTemplateId = r.Id,
                    TemplateName = r.VariableTemplate.TemplateName,
                    ActivityName = r.Activity.CtmsActivity.ActivityName,
                    Notes = r.VariableTemplate.Notes.Where(c => c.DeletedDate == null).Select(a => a.Note).ToList(),
                    VariableTemplateDetails = r.VariableTemplate.VariableTemplateDetails.Where(c => c.Variable.DeletedDate == null).ToList()
                }
            ).FirstOrDefault();

            if (result != null)
            {
                var variables = _context.StudyLevelFormVariable.Where(t => t.StudyLevelFormId == id && t.DeletedDate == null)
                    .Select(x => new StudyLevelFormVariableDto
                    {
                        StudyLevelFormId = x.StudyLevelFormId,
                        StudyLevelFormVariableId = x.Id,
                        Id = x.Id,
                        VariableName = x.VariableName,
                        VariableCode = x.VariableCode,
                        CollectionSource = x.CollectionSource,
                        ValidationType = x.ValidationType,
                        DataType = x.DataType,
                        Length = x.Length,
                        DefaultValue = string.IsNullOrEmpty(x.DefaultValue) && x.CollectionSource == CollectionSources.HorizontalScale ? "1" : x.DefaultValue,
                        LargeStep = x.LargeStep,
                        LowRangeValue = x.LowRangeValue,
                        HighRangeValue = x.HighRangeValue,
                        RelationStudyLevelFormVariableId = x.RelationStudyLevelFormVariableId,
                        PrintType = x.PrintType,
                        UnitName = x.Unit.UnitName,
                        DesignOrder = x.DesignOrder,
                        IsDocument = x.IsDocument,
                        VariableCategoryName = (_jwtTokenAccesser.Language != 1 ?
                        x.VariableCategory.VariableCategoryLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null && c.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : x.VariableCategory.CategoryName) ?? "",
                        SystemType = x.SystemType,
                        IsNa = x.IsNa,
                        DateValidate = x.DateValidate,
                        Alignment = x.Alignment ?? Alignment.Right,
                        StudyVersion = x.StudyVersion,
                        InActiveVersion = x.InActiveVersion,
                        Note = x.Note,
                        ValidationMessage = x.ValidationType == ValidationType.Required ? "This field is required" : "",
                        IsLevelNo = false,
                    }).OrderBy(r => r.DesignOrder).ToList();

                var values = _studyLevelFormVariableValueRepository.All.
                     Where(x => x.StudyLevelFormVariable.StudyLevelFormId == id && x.DeletedDate == null).Select(c => new StudyLevelFormVariableValueDto
                     {
                         Id = c.Id,
                         StudyLevelFormVariableId = c.StudyLevelFormVariableId,
                         ValueName = c.ValueName,
                         SeqNo = c.SeqNo,
                         Label = c.Label,
                         TableCollectionSource = c.TableCollectionSource,
                         Style = c.Style,
                     }).ToList();

                variables.ForEach(x =>
                {
                    x.Values = values.Where(c => c.StudyLevelFormVariableId == x.StudyLevelFormVariableId).OrderBy(c => c.SeqNo).ToList();
                });

                result.Variables = variables;
            }

            return result;
        }

        public StudyLevelForm GetTemplateForVerification(int ProjectId)
        {
            return All.Where(x => x.Activity.CtmsActivity.ActivityCode == "sm_001" && x.ProjectId == ProjectId && x.DeletedDate == null).FirstOrDefault();
        }

        public DesignVerificationApprovalTemplateDto GetReportFormVariableForVerification(int id)
        {
            var result = All.Where(t => t.Id == id).Include(x => x.Activity).ThenInclude(x => x.CtmsActivity)
                .Include(x => x.VariableTemplate).ThenInclude(d => d.VariableTemplateDetails)
                .Include(x => x.VariableTemplate).ThenInclude(d => d.Notes)
                .Select(r => new DesignVerificationApprovalTemplateDto
                {
                    Id = r.Id,
                    VariableTemplateId = r.VariableTemplateId,
                    TemplateName = r.VariableTemplate.TemplateName,
                    ActivityName = r.Activity.CtmsActivity.ActivityName,
                    Notes = r.VariableTemplate.Notes.Where(c => c.DeletedDate == null).Select(a => a.Note).ToList()
                }
            ).FirstOrDefault();

            if (result != null)
            {
                var variables = _context.StudyLevelFormVariable.Where(t => t.StudyLevelFormId == id && t.DeletedDate == null)
                    .Select(x => new VerificationApprovalVariableDto
                    {
                        StudyLevelFormId = x.StudyLevelFormId,
                        StudyLevelFormVariableId = x.Id,
                        Id = x.Id,
                        VariableName = x.VariableName,
                        VariableCode = x.VariableCode,
                        CollectionSource = x.CollectionSource,
                        ValidationType = x.ValidationType,
                        DataType = x.DataType,
                        Length = x.Length,
                        DefaultValue = string.IsNullOrEmpty(x.DefaultValue) && x.CollectionSource == CollectionSources.HorizontalScale ? "1" : x.DefaultValue,
                        LargeStep = x.LargeStep,
                        LowRangeValue = x.LowRangeValue,
                        HighRangeValue = x.HighRangeValue,
                        RelationStudyLevelFormVariableId = x.RelationStudyLevelFormVariableId,
                        PrintType = x.PrintType,
                        UnitName = x.Unit.UnitName,
                        DesignOrder = x.DesignOrder,
                        IsDocument = x.IsDocument,
                        VariableCategoryName = (_jwtTokenAccesser.Language != 1 ?
                        x.VariableCategory.VariableCategoryLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null && c.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : x.VariableCategory.CategoryName) ?? "",
                        SystemType = x.SystemType,
                        IsNa = x.IsNa,
                        DateValidate = x.DateValidate,
                        Alignment = x.Alignment ?? Alignment.Right,
                        StudyVersion = x.StudyVersion,
                        InActiveVersion = x.InActiveVersion,
                        Note = x.Note,
                        ValidationMessage = x.ValidationType == ValidationType.Required ? "This field is required" : "",
                    }).OrderBy(r => r.DesignOrder).ToList();

                var values = _studyLevelFormVariableValueRepository.All.
                     Where(x => x.StudyLevelFormVariable.StudyLevelFormId == id && x.DeletedDate == null).Select(c => new StudyLevelFormVariableValueDto
                     {
                         Id = c.Id,
                         StudyLevelFormVariableId = c.StudyLevelFormVariableId,
                         ValueName = c.ValueName,
                         SeqNo = c.SeqNo,
                         Label = c.Label,
                     }).ToList();

                variables.ForEach(x =>
                {
                    x.Values = values.Where(c => c.StudyLevelFormVariableId == x.StudyLevelFormVariableId).OrderBy(c => c.SeqNo).ToList();
                });

                result.Variables = variables;
            }
            return result;
        }
        public string CheckVerificationApproval(int id)
        {
            var VariableTemplate = _context.VerificationApprovalTemplate.Where(x => x.StudyLevelFormId == id
            && (x.ProductVerificationDetail.ProductReceipt.Status == ProductVerificationStatus.SentForApproval || x.ProductVerificationDetail.ProductReceipt.Status == ProductVerificationStatus.Approved)).FirstOrDefault();
            if (VariableTemplate != null)
                return "Study Level Form is in use. Cannot edit or delete!";

            return "";
        }
    }
}
