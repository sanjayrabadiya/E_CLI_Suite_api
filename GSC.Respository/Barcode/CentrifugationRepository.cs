
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
        public CentrifugationRepository(IGSCContext context)
            : base(context)
        {
        }

        public string Duplicate(BarcodeType objSave)
        {  
            if (All.Any(x => x.DeletedDate == null))
                return "Duplicate BarcodeType name : " + objSave.BarcodeTypeName;

            return "";
        }
    }
}
