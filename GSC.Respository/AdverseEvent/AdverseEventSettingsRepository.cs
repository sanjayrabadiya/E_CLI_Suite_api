﻿using AutoMapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.AdverseEvent;
using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.AdverseEvent
{
    public class AdverseEventSettingsRepository : GenericRespository<AdverseEventSettings>, IAdverseEventSettingsRepository
    {
        private readonly IGSCContext _context;
        private readonly IAdverseEventSettingsDetailRepository _adverseEventSettingsDetailRepository;
        private readonly IStudyVersionRepository _studyVersionRepository;
        public AdverseEventSettingsRepository(IGSCContext context,
            IAdverseEventSettingsDetailRepository adverseEventSettingsDetailRepository,
            IStudyVersionRepository studyVersionRepository) : base(context)
        {
            _context = context;
            _adverseEventSettingsDetailRepository = adverseEventSettingsDetailRepository;
            _studyVersionRepository = studyVersionRepository;

        }

        public IList<DropDownDto> GetVisitDropDownforAEReportingInvestigatorForm(int projectId)
        {
            var studyVersion = _studyVersionRepository.GetStudyVersionForLive(projectId);

            var visits = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
             && x.ProjectDesignPeriod.DeletedDate == null && x.ProjectDesignPeriod.ProjectDesign.DeletedDate == null
             && x.DeletedDate == null && (x.StudyVersion == null || x.StudyVersion <= studyVersion) &&
            (x.InActiveVersion == null || x.InActiveVersion > studyVersion))
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.DisplayName,
                }).Distinct().ToList();
            return visits;
        }

        public IList<DropDownDto> GetTemplateDropDownforPatientAEReporting(int projectId)
        {
            var appscreen = _context.AppScreen.Where(c => c.ScreenCode == "mnu_adverseevent").FirstOrDefault();
            var templates = _context.ProjectDesignTemplate.Where(x =>
                            x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
                            && x.VariableTemplate.AppScreenId == appscreen.Id
                            && x.IsParticipantView
                            && x.ProjectDesignVisit.DeletedDate == null
                            && x.ProjectDesignVisit.IsNonCRF
                            && x.ProjectDesignVisit.ProjectDesignPeriod.DeletedDate == null
                            && x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.DeletedDate == null
                            && x.DeletedDate == null)
                            .Select(x => new DropDownDto
                            {
                                Id = x.Id,
                                Value = x.TemplateName,
                            }).Distinct().ToList();
            return templates;
        }

        public IList<DropDownDto> GetTemplateDropDownforInvestigatorAEReporting(int visitId)
        {
            var templates = _context.ProjectDesignTemplate.Where(x => x.ProjectDesignVisitId == visitId && x.DeletedDate == null)
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.TemplateName,
                }).Distinct().ToList();
            return templates;
        }

        public IList<AdverseEventSettingsVariableValue> GetAdverseEventSettingsVariableValue(int projectDesignTemplateId)
        {

            var projectdesignvariable = _context.ProjectDesignVariable.Where(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null && x.CollectionSource == Helper.CollectionSources.RadioButton).FirstOrDefault();
            if (projectdesignvariable != null)
            {
                var projectdesignvariablevalues = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariableId == projectdesignvariable.Id)
                    .Select(x => new AdverseEventSettingsVariableValue
                    {
                        ProjectDesignVariableId = x.ProjectDesignVariableId,
                        ProjectDesignVariableValueId = x.Id,
                        Value = x.ValueName,
                        SeqNo = x.SeqNo,
                        SeveritySeqNo = x.SeqNo,
                        Severity = x.SeqNo == 1 ? "Low" : (x.SeqNo == 2 ? "Medium" : "High")
                    }).ToList();
                return projectdesignvariablevalues;
            }
            return new List<AdverseEventSettingsVariableValue>();
        }

      
        public AdverseEventSettingsListDto GetData(int projectId)
        {
            var adverseEventSettingsDto = _context.AdverseEventSettings.Where(x => x.ProjectId == projectId && x.DeletedDate == null)
                .Select(x => new AdverseEventSettingsListDto
                {
                    Id = x.Id,
                    ProjectDesignVisitIdInvestigator = x.ProjectDesignVisitIdInvestigator,
                    ProjectDesignTemplateIdInvestigator = x.ProjectDesignTemplateIdInvestigator,
                    ProjectDesignTemplateIdPatient = x.ProjectDesignTemplateIdPatient,
                    adverseEventSettingsDetails = x.adverseEventSettingsDetails != null ? x.adverseEventSettingsDetails.Select(c => new AdverseEventSettingsVariableValue
                    {
                        Id = c.Id,
                        ProjectDesignVariableId = c.ProjectDesignVariableId,
                        ProjectDesignVariableValueId = c.ProjectDesignVariableValueId,
                        AdverseEventSettingsId = c.AdverseEventSettingsId,
                        SeqNo = c.SeveritySeqNo,
                        SeveritySeqNo = c.SeveritySeqNo,
                        Severity = c.Severity,
                        Value = c.Value
                    }).ToList() : null

                }).FirstOrDefault();

            return adverseEventSettingsDto;
        }

        public void RemoveExistingAdverseDetail(int id)
        {
            var data = _adverseEventSettingsDetailRepository.All.Where(x => x.AdverseEventSettingsId == id).ToList();
            if (data.Any())
            {
                _context.AdverseEventSettingsDetails.RemoveRange(data);
            }
        }

        public bool IsvalidPatientTemplate(int projectDesignTemplateId)
        {
            if (_context.ProjectDesignVariable.Count(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null && x.CollectionSource == Helper.CollectionSources.MultilineTextBox) > 1 ||
                _context.ProjectDesignVariable.Count(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null && x.CollectionSource == Helper.CollectionSources.MultilineTextBox) == 0)
                return false;
            if (_context.ProjectDesignVariable.Count(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null && x.CollectionSource == Helper.CollectionSources.DateTime) > 1 ||
                _context.ProjectDesignVariable.Count(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null && x.CollectionSource == Helper.CollectionSources.DateTime) == 0)
                return false;
            if (_context.ProjectDesignVariable.Count(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null && x.CollectionSource == Helper.CollectionSources.RadioButton) > 1 ||
                _context.ProjectDesignVariable.Count(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null && x.CollectionSource == Helper.CollectionSources.RadioButton) == 0)
                return false;
            var data = _context.ProjectDesignVariable.Where(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null
            && x.CollectionSource != Helper.CollectionSources.RadioButton && x.CollectionSource == Helper.CollectionSources.DateTime && x.CollectionSource == Helper.CollectionSources.RadioButton).FirstOrDefault();
            if (data != null)
            {
                return false;
            }
            return true;
        }
    }
}
