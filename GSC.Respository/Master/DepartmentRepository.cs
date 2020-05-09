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
    public class DepartmentRepository : GenericRespository<Department, GscContext>, IDepartmentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public DepartmentRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        public List<DropDownDto> GetDepartmentDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.DepartmentName, Code = c.DepartmentCode})
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Department objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.DepartmentCode == objSave.DepartmentCode && x.DeletedDate == null))
                return "Duplicate Department code : " + objSave.DepartmentCode;

            if (All.Any(x => x.Id != objSave.Id && x.DepartmentName == objSave.DepartmentName && x.DeletedDate == null))
                return "Duplicate Department name : " + objSave.DepartmentName;

            return "";
        }
    }
}