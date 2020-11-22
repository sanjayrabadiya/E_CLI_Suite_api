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
    public class RegulatoryTypeRepository : GenericRespository<RegulatoryType, GscContext>, IRegulatoryTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public RegulatoryTypeRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetRegulatoryTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.RegulatoryTypeName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(RegulatoryType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.RegulatoryTypeName == objSave.RegulatoryTypeName && x.DeletedDate == null))
                return "Duplicate Regulatory name : " + objSave.RegulatoryTypeName;
            return "";
        }

        public List<RegulatoryTypeGridDto> GetRegulatoryTypeList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<RegulatoryTypeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}