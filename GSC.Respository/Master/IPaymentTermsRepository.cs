using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.Master
{
    public interface IPaymentTermsRepository : IGenericRepository<PaymentTerms>
    {
        string Duplicate(PaymentTerms objSave);
        List<PaymentTermsGridDto> GetPaymentTermsList(bool isDeleted);
    }
}
