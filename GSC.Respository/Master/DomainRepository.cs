using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class DomainRepository : GenericRespository<Data.Entities.Master.Domain>, IDomainRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public DomainRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public string ValidateDomain(Data.Entities.Master.Domain objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.DomainCode == objSave.DomainCode.Trim() && x.DeletedDate == null))
                return "Duplicate Domain Code : " + objSave.DomainCode;

            if (All.Any(x => x.Id != objSave.Id && x.DomainName == objSave.DomainName.Trim() && x.DeletedDate == null))
                return "Duplicate Domain Name : " + objSave.DomainName;

            return "";
        }

        public List<DropDownDto> GetDomainDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.DomainName, Code = c.DomainCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }


        public List<DomainGridDto> GetDomainList(bool isDeleted)
        {
         
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<DomainGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }


        public List<DropDownDto> GetDomainByProjectDesignDropDown(int projectDesignId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null
                                                                                        && _context.ProjectDesignTemplate
                                                                                            .Any(r => r.DomainId == x.Id
                                                                                                      && r
                                                                                                          .ProjectDesignVisit
                                                                                                          .ProjectDesignPeriod
                                                                                                          .ProjectDesignId ==
                                                                                                      projectDesignId
                                                                                                      && r
                                                                                                          .DeletedDate ==
                                                                                                      null))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.DomainName, Code = c.DomainCode })
                .OrderBy(o => o.Value).ToList();
        }

        public List<DomainDto> GetDomainAll(bool isDeleted)
        {
            var domains = All.Where(x =>
                (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).Select(c => new DomainDto
            {
                Id = c.Id,
                DomainName = c.DomainName,
                DomainCode = c.DomainCode,
                DomainClassName = c.DomainClass.DomainClassName,
                IsDeleted = c.DeletedDate != null
            }).ToList();

            return domains;
        }
    }
}