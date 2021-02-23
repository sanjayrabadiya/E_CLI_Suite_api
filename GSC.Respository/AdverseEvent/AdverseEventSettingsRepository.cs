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
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        public AdverseEventSettingsRepository(IGSCContext context, IMapper mapper,
            IProjectDesignTemplateRepository projectDesignTemplateRepository) : base(context)
        {
            _context = context;
            _mapper = mapper;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
        }

        public IList<DropDownDto> GetVisitDropDownforAEReportingInvestigatorForm(int projectId)
        {
            var projectdesigns = _context.ProjectDesign.Where(x => x.ProjectId == projectId && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();
            var projectdesignperiods = _context.ProjectDesignPeriod.Where(x => projectdesigns.Contains(x.ProjectDesignId) && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();
            var visits = _context.ProjectDesignVisit.Where(x => projectdesignperiods.Contains(x.ProjectDesignPeriodId) && x.DeletedDate == null).ToList()
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.DisplayName,
                }).Distinct().ToList();
            return visits;
        }

        public IList<DropDownDto> GetVisitDropDownforAEReportingPatientForm(int projectId)
        {
            var projectdesigns = _context.ProjectDesign.Where(x => x.ProjectId == projectId && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();
            var projectdesignperiods = _context.ProjectDesignPeriod.Where(x => projectdesigns.Contains(x.ProjectDesignId) && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();
            var visits = _context.ProjectDesignVisit.Where(x => projectdesignperiods.Contains(x.ProjectDesignPeriodId) && x.DeletedDate == null && x.IsNonCRF == true).ToList()
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.DisplayName,
                }).Distinct().ToList();
            return visits;
        }
        public IList<DropDownDto> GetTemplateDropDownforPatientAEReporting(int visitId)
        {
            var templates = _context.ProjectDesignTemplate.Where(x => x.ProjectDesignVisitId == visitId && x.DeletedDate == null).ToList()
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.TemplateName,
                }).Distinct().ToList();
            return templates;
        }

        public IList<DropDownDto> GetTemplateDropDownforInvestigatorAEReporting(int visitId)
        {
            var templates = _context.ProjectDesignTemplate.Where(x => x.ProjectDesignVisitId == visitId && x.DeletedDate == null && x.IsRepeated == true).ToList()
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.TemplateName,
                }).Distinct().ToList();
            return templates;
        }

        public IList<AdverseEventSettingsVariableValue> GetAdverseEventSettingsVariableValue(int projectDesignTemplateId)
        {
            var projectdesignvariableid = _context.ProjectDesignVariable.Where(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.CollectionSource == Helper.CollectionSources.RadioButton).ToList().FirstOrDefault().Id;
            var projectdesignvariablevalues = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariableId == projectdesignvariableid).ToList()
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

        public AdverseEventSettingsDto GetData(int projectId)
        {
            var adverseEventSettings = FindBy(x => x.ProjectId == projectId).ToList().FirstOrDefault();
            var adverseEventSettingsDto = _mapper.Map<AdverseEventSettingsDto>(adverseEventSettings);
            if (adverseEventSettingsDto != null)
            {
                adverseEventSettingsDto.ProjectDesignVisitIdInvestigator = _projectDesignTemplateRepository.Find(adverseEventSettingsDto.ProjectDesignTemplateIdInvestigator).ProjectDesignVisitId;
                adverseEventSettingsDto.ProjectDesignVisitIdPatient = _projectDesignTemplateRepository.Find(adverseEventSettingsDto.ProjectDesignTemplateIdPatient).ProjectDesignVisitId;
                List<AdverseEventSettingsVariableValue> variableValues = new List<AdverseEventSettingsVariableValue>();
                var projectdesignvariableid = _context.ProjectDesignVariable.Where(x => x.ProjectDesignTemplateId == adverseEventSettingsDto.ProjectDesignTemplateIdPatient && x.CollectionSource == Helper.CollectionSources.RadioButton).ToList().FirstOrDefault().Id;
                var projectdesignvariablevalues = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariableId == projectdesignvariableid).ToList();
                AdverseEventSettingsVariableValue obj1 = new AdverseEventSettingsVariableValue();
                obj1.ProjectDesignVariableId = projectdesignvariableid;
                obj1.ProjectDesignVariableValueId = adverseEventSettingsDto.SeveritySeqNo1;
                obj1.Severity = "Low";
                obj1.Value = projectdesignvariablevalues.Where(x => x.Id == obj1.ProjectDesignVariableValueId).ToList().FirstOrDefault().ValueName;
                obj1.SeveritySeqNo = 1;
                variableValues.Add(obj1);
                AdverseEventSettingsVariableValue obj2 = new AdverseEventSettingsVariableValue();
                obj2.ProjectDesignVariableId = projectdesignvariableid;
                obj2.ProjectDesignVariableValueId = adverseEventSettingsDto.SeveritySeqNo2;
                obj2.Severity = "Medium";
                obj2.Value = projectdesignvariablevalues.Where(x => x.Id == obj2.ProjectDesignVariableValueId).ToList().FirstOrDefault().ValueName;
                obj2.SeveritySeqNo = 2;
                variableValues.Add(obj2);
                AdverseEventSettingsVariableValue obj3 = new AdverseEventSettingsVariableValue();
                obj3.ProjectDesignVariableId = projectdesignvariableid;
                obj3.ProjectDesignVariableValueId = adverseEventSettingsDto.SeveritySeqNo3;
                obj3.Severity = "High";
                obj3.Value = projectdesignvariablevalues.Where(x => x.Id == obj3.ProjectDesignVariableValueId).ToList().FirstOrDefault().ValueName;
                obj3.SeveritySeqNo = 3;
                variableValues.Add(obj3);
                adverseEventSettingsDto.variableValues = variableValues;
            }
            return adverseEventSettingsDto;
        }

        
    }
}
