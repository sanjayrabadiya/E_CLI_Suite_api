using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Master
{
    public class RaceRepository : GenericRespository<Race, GscContext>, IRaceRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public RaceRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetRaceDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.RaceName,IsDeleted=c.DeletedDate!=null}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Race objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.RaceName == objSave.RaceName && x.DeletedDate == null))
                return "Duplicate Race name : " + objSave.RaceName;
            return "";
        }

        public List<RaceGridDto> GetRaceList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<RaceGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}