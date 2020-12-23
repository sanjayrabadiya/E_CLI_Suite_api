using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class DepartmentRepository : GenericRespository<Department>, IDepartmentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public DepartmentRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
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
            if (All.Any(x => x.Id != objSave.Id && x.DepartmentCode == objSave.DepartmentCode.Trim() && x.DeletedDate == null))
                return "Duplicate Department code : " + objSave.DepartmentCode;

            if (All.Any(x => x.Id != objSave.Id && x.DepartmentName == objSave.DepartmentName.Trim() && x.DeletedDate == null))
                return "Duplicate Department name : " + objSave.DepartmentName;

            return "";
        }

        public List<DepartmentGridDto> GetDepartmentList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
       ProjectTo<DepartmentGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}