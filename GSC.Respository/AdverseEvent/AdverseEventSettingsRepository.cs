using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.AdverseEvent;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.AdverseEvent
{
    public class AdverseEventSettingsRepository : GenericRespository<AdverseEventSettings>, IAdverseEventSettingsRepository
    {
        private readonly IGSCContext _context;
        public AdverseEventSettingsRepository(IGSCContext context) : base(context)
        {
            _context = context;
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
        public IList<DropDownDto> GetTemplateDropDownforAEReporting(int visitId)
        {
            var templates = _context.ProjectDesignTemplate.Where(x => x.ProjectDesignVisitId == visitId && x.DeletedDate == null).ToList()
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.TemplateName,
                }).Distinct().ToList();
            return templates;
        }

    }
}
