using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class InvestigatorContactRepository : GenericRespository<InvestigatorContact>,
        IInvestigatorContactRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public InvestigatorContactRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetInvestigatorContactDropDown(int cityId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.NameOfInvestigator })
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(InvestigatorContact objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.RegistrationNumber == objSave.RegistrationNumber.Trim() && x.DeletedDate == null))
                return "Duplicate RegistrationNumber : " + objSave.RegistrationNumber;
            return "";
        }

        public List<DropDownDto> GetAllInvestigatorContactDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.NameOfInvestigator, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }
    }
}