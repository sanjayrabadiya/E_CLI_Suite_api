using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Barcode
{
    public class BarcodeTypeRepository : GenericRespository<BarcodeType>, IBarcodeTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public BarcodeTypeRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetBarcodeTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.BarcodeTypeName})
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(BarcodeType objSave)
        {           
            if (All.Any(
                x => x.Id != objSave.Id && x.BarcodeTypeName == objSave.BarcodeTypeName.Trim() && x.DeletedDate == null))
                return "Duplicate BarcodeType name : " + objSave.BarcodeTypeName;

            return "";
        }
    }
}