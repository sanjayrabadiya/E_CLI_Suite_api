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
    public class BlockCategoryRepository : GenericRespository<BlockCategory>, IBlockCategoryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public BlockCategoryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetBlockCategoryDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.BlockCategoryName, Code = c.BlockCode,IsDeleted=c.DeletedDate!=null})
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(BlockCategory objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.BlockCode == objSave.BlockCode.Trim() && x.DeletedDate == null))
                return "Duplicate Block code : " + objSave.BlockCode;
            if (All.Any(x =>
                x.Id != objSave.Id && x.BlockCategoryName == objSave.BlockCategoryName.Trim() && x.DeletedDate == null))
                return "Duplicate Block Category name : " + objSave.BlockCategoryName;
            return "";
        }

        public List<BlockCategoryGridDto> GetBlockCategoryList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<BlockCategoryGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}