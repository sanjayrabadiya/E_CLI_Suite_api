using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Location;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.ProjectRight;

namespace GSC.Respository.Master
{
    public class CountryRepository : GenericRespository<Country, GscContext>, ICountryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;

        public CountryRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
        }

        public List<DropDownDto> GetCountryDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.CountryName, Code = c.CountryCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetProjectCountryDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.CountryName, Code = c.CountryCode })
                .OrderBy(o => o.Value).ToList();
        }

        public string DuplicateCountry(Country objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.CountryCode == objSave.CountryCode && x.DeletedDate == null))
                return "Duplicate Country code : " + objSave.CountryCode;

            if (All.Any(x => x.Id != objSave.Id && x.CountryName == objSave.CountryName && x.DeletedDate == null))
                return "Duplicate Country name : " + objSave.CountryName;

            if (!string.IsNullOrEmpty(objSave.CountryCallingCode))
                if (All.Any(x =>
                    x.Id != objSave.Id && x.CountryCallingCode == objSave.CountryCallingCode && x.DeletedDate == null))
                    return "Duplicate Country calling code : " + objSave.CountryCallingCode;

            return "";
        }

        public List<DropDownDto> GetCountryByParentProjectIdDropDown(int ProjectDesignId)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            var ParentProjectId = Context.ProjectDesign.Find(ProjectDesignId).ProjectId;

            var countries = (from P in Context.Project
                             join c in Context.Country on P.CountryId equals c.Id
                             where (P.ParentProjectId == ParentProjectId || P.Id == ParentProjectId && P.DeletedDate == null) && projectList.Any(c => c == P.Id)
                             select new DropDownDto
                             {
                                 Id = c.Id,
                                 Value = c.CountryName,
                                 Code = c.CountryCode
                             }).Distinct().OrderBy(o => o.Value).ToList();
            return countries.GroupBy(x => x.Id, (key, group) => group.First()).ToList();
        }

    }
}