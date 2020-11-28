using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class VariableTemplateRepository : GenericRespository<VariableTemplate>,
        IVariableTemplateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public VariableTemplateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public List<DropDownDto> GetVariableTemplateDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.TemplateName, Code = c.TemplateCode })
                .OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetVariableTemplateByDomainId(int domainId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null &&
                    x.DomainId == domainId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.TemplateName, Code = c.TemplateCode })
                .OrderBy(o => o.Value).ToList();
        }

        public VariableTemplate GetTemplate(int id)
        {
            var template = _context.VariableTemplate.Where(t => t.Id == id)
                .Include(d => d.VariableTemplateDetails)
                .Include(t => t.Domain)
                .Include(t => t.Notes)
                .FirstOrDefault();

            template.Notes = template.Notes.Where(t => t.DeletedDate == null && t.IsPreview).ToList();
            template.VariableTemplateDetails = template.VariableTemplateDetails.Where(t => t.DeletedDate == null)
                .OrderBy(t => t.SeqNo).ToList();
            template.VariableTemplateDetails.ForEach(detail =>
            {
                detail.Variable = _context.Variable.Where(t => t.Id == detail.VariableId)
                    .Include(variable => variable.Values)
                    .Include(r=> r.Remarks)
                    .Include(t => t.Unit)
                    .FirstOrDefault();

                detail.Variable.Values = detail.Variable.Values.Where(t => t.DeletedDate == null).ToList();
                detail.Variable.Remarks = detail.Variable.Remarks.Where(t => t.DeletedDate == null).ToList();
                detail.VariableCategoryName = detail.Variable.VariableCategoryId == null
                    ? ""
                    : _context.VariableCategory.Where(t => t.Id == detail.Variable.VariableCategoryId).FirstOrDefault()?.CategoryName;
            });

            return template;
        }

        public string Duplicate(VariableTemplate objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TemplateCode == objSave.TemplateCode && x.DeletedDate == null))
                return "Duplicate Template code : " + objSave.TemplateCode;

            if (All.Any(x => x.Id != objSave.Id && x.ActivityName == objSave.ActivityName && x.DeletedDate == null && !string.IsNullOrEmpty(x.ActivityName)))
                return "Duplicate Activity name : " + objSave.ActivityName;

            if (All.Any(x => x.Id != objSave.Id && x.TemplateName == objSave.TemplateName && x.DeletedDate == null))
                return "Duplicate Template name : " + objSave.TemplateName;

            return "";
        }
    }
}