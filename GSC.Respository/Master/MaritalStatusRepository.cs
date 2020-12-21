using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class MaritalStatusRepository : GenericRespository<MaritalStatus>, IMaritalStatusRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public MaritalStatusRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetMaritalStatusDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.MaritalStatusName,IsDeleted=c.DeletedDate!=null}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(MaritalStatus objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.MaritalStatusName == objSave.MaritalStatusName.Trim() && x.DeletedDate == null))
                return "Duplicate MaritalStatus name : " + objSave.MaritalStatusName;
            return "";
        }

        public List<MaritalStatusGridDto> GetMaritalStatusList(bool isDeleted)
        {
            return All.Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<MaritalStatusGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}