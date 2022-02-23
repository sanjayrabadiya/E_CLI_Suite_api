using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IAdverseEventSettingsDetailRepository _adverseEventSettingsDetailRepository;
        public AdverseEventSettingsRepository(IGSCContext context, IMapper mapper,
            IAdverseEventSettingsDetailRepository adverseEventSettingsDetailRepository) : base(context)
        {
            _context = context;
            _mapper = mapper;
            _adverseEventSettingsDetailRepository = adverseEventSettingsDetailRepository;

        }

        public IList<DropDownDto> GetVisitDropDownforAEReportingInvestigatorForm(int projectId)
        {
            var visits = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
             && x.ProjectDesignPeriod.DeletedDate == null && x.ProjectDesignPeriod.ProjectDesign.DeletedDate == null
             && x.DeletedDate == null)
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
                            && x.IsParticipantView == true
                            && x.ProjectDesignVisit.DeletedDate == null
                            && x.ProjectDesignVisit.IsNonCRF == true
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
            return null;
        }

        public AdverseEventSettingsListDto GetData(int projectId)
        {
            var adverseEventSettingsDto = _context.AdverseEventSettings.Where(x => x.ProjectId == projectId)
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
            if (data != null)
            {
                _context.AdverseEventSettingsDetails.RemoveRange(data);
                _context.Save();
            }
        }

        public bool IsvalidPatientTemplate(int projectDesignTemplateId)
        {
            if (_context.ProjectDesignVariable.Count(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null && x.CollectionSource == Helper.CollectionSources.MultilineTextBox) > 1)
                return false;
            if (_context.ProjectDesignVariable.Count(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null && x.CollectionSource == Helper.CollectionSources.DateTime) > 1)
                return false;
            if (_context.ProjectDesignVariable.Count(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null && x.CollectionSource == Helper.CollectionSources.RadioButton) > 1)
                return false;

            return true;
        }


    }
}
