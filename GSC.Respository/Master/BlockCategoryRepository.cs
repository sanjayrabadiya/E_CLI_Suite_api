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
    public class BlockCategoryRepository : GenericRespository<BlockCategory, GscContext>, IBlockCategoryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public BlockCategoryRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
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
            if (All.Any(x => x.Id != objSave.Id && x.BlockCode == objSave.BlockCode && x.DeletedDate == null))
                return "Duplicate Block code : " + objSave.BlockCode;
            if (All.Any(x =>
                x.Id != objSave.Id && x.BlockCategoryName == objSave.BlockCategoryName && x.DeletedDate == null))
                return "Duplicate Block Category name : " + objSave.BlockCategoryName;
            return "";
        }
    }
}