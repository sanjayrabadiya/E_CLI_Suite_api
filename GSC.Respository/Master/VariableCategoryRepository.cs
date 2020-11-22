using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Master
{
    public class VariableCategoryRepository : GenericRespository<VariableCategory>,
        IVariableCategoryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public VariableCategoryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetVariableCategoryDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.CategoryName, Code = c.CategoryCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(VariableCategory objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.CategoryCode == objSave.CategoryCode && x.DeletedDate == null))
                return "Duplicate Variable Category code : " + objSave.CategoryCode;
            if (All.Any(x => x.Id != objSave.Id && x.CategoryName == objSave.CategoryName && x.DeletedDate == null))
                return "Duplicate Variable Category name : " + objSave.CategoryName;
            return "";
        }

        public List<VariableCategoryGridDto> GetVariableCategoryList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<VariableCategoryGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}