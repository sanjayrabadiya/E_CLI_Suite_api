using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Configuration
{
    public class PharmacyConfigRepository : GenericRespository<PharmacyConfig, GscContext>, IPharmacyConfigRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public PharmacyConfigRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetVariableTemplateByFormId(int formId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.FormId == formId && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.VariableTemplate.TemplateName})
                .OrderBy(o => o.Value).ToList();
        }


        public List<DropDownDto> GetTemplateByForm(int formId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.FormName}).OrderBy(o => o.Value).ToList();
        }
    }
}