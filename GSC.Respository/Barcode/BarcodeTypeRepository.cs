using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Barcode
{
    public class BarcodeTypeRepository : GenericRespository<BarcodeType, GscContext>, IBarcodeTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public BarcodeTypeRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetBarcodeTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.BarcodeTypeName, Code = c.BarcodeTypeCode})
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(BarcodeType objSave)
        {
            if (All.Any(
                x => x.Id != objSave.Id && x.BarcodeTypeCode == objSave.BarcodeTypeCode && x.DeletedDate == null))
                return "Duplicate BarcodeType code : " + objSave.BarcodeTypeCode;

            if (All.Any(
                x => x.Id != objSave.Id && x.BarcodeTypeName == objSave.BarcodeTypeName && x.DeletedDate == null))
                return "Duplicate BarcodeType name : " + objSave.BarcodeTypeName;

            return "";
        }
    }
}