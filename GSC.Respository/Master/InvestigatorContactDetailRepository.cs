using System;
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
    public class InvestigatorContactDetailRepository : GenericRespository<InvestigatorContactDetail>, IInvestigatorContactDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public InvestigatorContactDetailRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
             IMapper mapper)
    : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public string DuplicateContact(InvestigatorContactDetail objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ContactNo == objSave.ContactNo && x.DeletedDate == null))
                return "Duplicate Contact No : " + objSave.ContactNo;

            return "";
        }

        public IList<InvestigatorContactDetailGridDto> GetContactList(int InvestigatorContactId, bool isDeleted)
        {
            return All.Where(x => x.InvestigatorContactId == InvestigatorContactId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                     ProjectTo<InvestigatorContactDetailGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<DropDownDto> GetInvestigatorContactDetailDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ContactName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
