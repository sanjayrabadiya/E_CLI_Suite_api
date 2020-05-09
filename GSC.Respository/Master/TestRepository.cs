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
    public class TestRepository : GenericRespository<Test, GscContext>, ITestRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public TestRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetTestDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TestName}).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetTestDropDownByTestGroup(int id)
        {
            return All.Where(x => x.TestGroupId == id && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TestName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Test objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TestName == objSave.TestName && x.DeletedDate == null))
                return "Duplicate Test name : " + objSave.TestName;
            return "";
        }
    }
}