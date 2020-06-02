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
    public class ClientTypeRepository : GenericRespository<ClientType, GscContext>, IClientTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ClientTypeRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public string Duplicate(ClientType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ClientTypeName == objSave.ClientTypeName && x.DeletedDate == null))
                return "Duplicate Client Type name : " + objSave.ClientTypeName;
            return "";
        }

        public List<DropDownDto> GetClientTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.ClientTypeName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}