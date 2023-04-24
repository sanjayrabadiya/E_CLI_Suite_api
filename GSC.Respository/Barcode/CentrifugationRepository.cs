
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Barcode
{
    public class CentrifugationRepository : GenericRespository<Centrifugation>, ICentrifugationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public CentrifugationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public string Duplicate(BarcodeType objSave)
        {
            //if (All.Any(
            //    x => x.Id != objSave.Id && x.BarcodeTypeCode == objSave.BarcodeTypeCode.Trim() && x.DeletedDate == null))
            //    return "Duplicate BarcodeType code : " + objSave.BarcodeTypeCode;

            if (All.Any(x => x.DeletedDate == null))
                return "Duplicate BarcodeType name : " + objSave.BarcodeTypeName;

            return "";
        }
    }
}
