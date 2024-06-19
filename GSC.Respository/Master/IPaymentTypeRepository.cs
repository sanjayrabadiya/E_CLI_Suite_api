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
    public interface IPaymentTypeRepository : IGenericRepository<PaymentType>
    {
        string Duplicate(PaymentType objSave);
        List<PaymentTypeGridDto> GetPaymentTypeList(bool isDeleted);
    }
}
