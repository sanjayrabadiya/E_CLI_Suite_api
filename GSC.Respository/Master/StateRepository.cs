using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Location;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Location;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class StateRepository : GenericRespository<State, GscContext>, IStateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public StateRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetStateDropDown(int countryId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) &&
                    x.CountryId == countryId)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.StateName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string DuplicateState(State objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.StateName == objSave.StateName && x.DeletedDate == null))
                return "Duplicate State name : " + objSave.StateName;

            return "";
        }

        public List<StateGridDto> GetStateList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                  ProjectTo<StateGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}