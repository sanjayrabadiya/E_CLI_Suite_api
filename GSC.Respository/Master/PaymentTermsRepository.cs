using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.Master
{
    public class PaymentTermsRepository : GenericRespository<PaymentTerms>, IPaymentTermsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public PaymentTermsRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public string Duplicate(PaymentTerms objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.Terms == objSave.Terms && x.DeletedDate == null))
                return "Duplicate payment terms : " + objSave.Terms;
            return "";
        }

        public List<PaymentTermsGridDto> GetPaymentTermsList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<PaymentTermsGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<DropDownDto> GetPaymentTermsDropDown()
        {
            return All.Where(x =>x.DeletedDate==null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.Terms.ToString(), IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
