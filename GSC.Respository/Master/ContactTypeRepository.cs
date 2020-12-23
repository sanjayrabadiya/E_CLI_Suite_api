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
    public class ContactTypeRepository : GenericRespository<ContactType>, IContactTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public ContactTypeRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetContactTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TypeName, Code = c.ContactCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(ContactType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ContactCode == objSave.ContactCode.Trim() && x.DeletedDate == null))
                return "Duplicate Contact code : " + objSave.ContactCode;
            if (All.Any(x => x.Id != objSave.Id && x.TypeName == objSave.TypeName.Trim() && x.DeletedDate == null))
                return "Duplicate ContactType name : " + objSave.TypeName;
            return "";
        }

        public List<ContactTypeGridDto> GetContactTypeList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
       ProjectTo<ContactTypeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}