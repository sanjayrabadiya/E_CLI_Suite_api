using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Configuration
{
    public class CompanyRepository : GenericRespository<Company>, ICompanyRepository
    {
        private readonly IMapper _mapper;

        public CompanyRepository(IGSCContext context,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }

        public IList<CompanyGridDto> GetCompanies(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
            ProjectTo<CompanyGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();           
        }

        public List<DropDownDto> GetCompanyDropDown()
        {
            return All.Where(x => x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.CompanyName, Code = c.CompanyCode })
                .OrderBy(o => o.Value).ToList();
        }
    }
}