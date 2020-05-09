using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class TestGroupRepository : GenericRespository<TestGroup, GscContext>, ITestGroupRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public TestGroupRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetTestGroupDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TestGroupName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(TestGroup objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TestGroupName == objSave.TestGroupName && x.DeletedDate == null))
                return "Duplicate Testgroup name : " + objSave.TestGroupName;
            return "";
        }
    }
}