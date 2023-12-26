using GSC.Common.GenericRespository;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Barcode
{
    public class PharmacyBarcodeDisplayInfoRepository : GenericRespository<PharmacyBarcodeDisplayInfo>, IPharmacyBarcodeDisplayInfoRepository
    {
        public PharmacyBarcodeDisplayInfoRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) :
           base(context)
        {
        }
    }
}
