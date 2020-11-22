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
    public class DomainClassRepository : GenericRespository<DomainClass, GscContext>, IDomainClassRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public DomainClassRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }


        public string ValidateDomainClass(DomainClass objSave)
        {
            if (All.Any(
                x => x.Id != objSave.Id && x.DomainClassCode == objSave.DomainClassCode && x.DeletedDate == null))
                return "Duplicate Domain Class Code : " + objSave.DomainClassCode;

            if (All.Any(x =>
                x.Id != objSave.Id && x.DomainClassName == objSave.DomainClassName &&
                x.DomainClassCode == objSave.DomainClassCode && x.DeletedDate == null))
                return "Duplicate Domain Class Name : " + objSave.DomainClassName;

            return "";
        }

        public List<DomainClassGridDto> GetDomainClassList(bool isDeleted)
        {

            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<DomainClassGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }

        public List<DropDownDto> GetDomainClassDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.DomainClassName, Code = c.DomainClassCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }
    }
}