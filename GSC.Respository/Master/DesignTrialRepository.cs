using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class DesignTrialRepository : GenericRespository<DesignTrial, GscContext>, IDesignTrialRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public DesignTrialRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetDesignTrialDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.DesignTrialName}).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetDesignTrialDropDownByTrialType(int id)
        {
            return All.Where(x => x.TrialTypeId == id && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.DesignTrialName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(DesignTrial objSave)
        {
            if (All.Any(
                x => x.Id != objSave.Id && x.DesignTrialCode == objSave.DesignTrialCode && x.DeletedDate == null))
                return "Duplicate DesignTrial code : " + objSave.DesignTrialCode;

            if (All.Any(
                x => x.Id != objSave.Id && x.DesignTrialName == objSave.DesignTrialName && x.DeletedDate == null))
                return "Duplicate DesignTrial name : " + objSave.DesignTrialName;
            return "";
        }
    }
}