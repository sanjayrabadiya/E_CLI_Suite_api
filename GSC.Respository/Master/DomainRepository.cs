using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class DomainRepository : GenericRespository<Data.Entities.Master.Domain, GscContext>, IDomainRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public DomainRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public string ValidateDomain(Data.Entities.Master.Domain objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.DomainCode == objSave.DomainCode && x.DeletedDate == null))
                return "Duplicate Domain Code : " + objSave.DomainCode;

            if (All.Any(x => x.Id != objSave.Id && x.DomainName == objSave.DomainName && x.DeletedDate == null))
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
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).Select(c => new DomainGridDto
            {
                Id = c.Id,
                DomainCode = c.DomainCode,
                DomainName = c.DomainName,
                IsDeleted = c.DeletedDate != null,
                DomainClassName = c.DomainClass.DomainClassName,
                CreatedByUser = c.CreatedByUser.UserName,
                DeletedByUser = c.DeletedByUser.UserName,
                ModifiedByUser = c.ModifiedByUser.UserName,
                CreatedDate = c.CreatedDate,
                ModifiedDate = c.ModifiedDate

            }).OrderByDescending(x => x.Id).ToList();

        }


        public List<DropDownDto> GetDomainByProjectDesignDropDown(int projectDesignId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null
                                                                                        && Context.ProjectDesignTemplate
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