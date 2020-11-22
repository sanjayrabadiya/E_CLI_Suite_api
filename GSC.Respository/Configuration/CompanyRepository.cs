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
using GSC.Shared;
using GSC.Shared.DocumentService;

namespace GSC.Respository.Configuration
{
    public class CompanyRepository : GenericRespository<Company, GscContext>, ICompanyRepository
    {
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IMapper _mapper;

        public CompanyRepository(IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IUploadSettingRepository uploadSettingRepository)
            : base(uow, jwtTokenAccesser)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _mapper = mapper;
        }

        public IList<CompanyGridDto> GetCompanies(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
            ProjectTo<CompanyGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            //var companies = FindByInclude(t => isDeleted ? t.DeletedDate != null : t.DeletedDate == null, t => t.Location).Select(s => new CompanyDto
            //{
            //    Id = s.Id,
            //    CompanyCode = s.CompanyCode,
            //    CompanyName = s.CompanyName,
            //    Phone1 = s.Phone1,
            //    Phone2 = s.Phone2,
            //    Location = s.Location,
            //    Logo = s.Logo,
            //    IsDeleted = s.DeletedDate != null
            //}).OrderByDescending(t => t.Id).ToList();
            //var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            //foreach (var company in companies)
            //{
            //    company.LogoPath = imageUrl + (company.Logo ?? DocumentService.DefulatLogo);

            //    if (company.Location == null)
            //        continue;

            //    var id = company.Location.CountryId;
            //    company.Location.CountryName = id > 0 ? Context.Country.Find(id).CountryName : "";

            //    id = company.Location.StateId;
            //    company.Location.StateName = id > 0 ? Context.State.Find(id).StateName : "";

            //    id = company.Location.CityId;
            //    company.Location.CityName = id > 0 ? Context.City.Find(id).CityName : "";
            //}

            //return companies;
        }

        public List<DropDownDto> GetCompanyDropDown()
        {
            return All.Where(x => x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.CompanyName, Code = c.CompanyCode })
                .OrderBy(o => o.Value).ToList();
        }
    }
}