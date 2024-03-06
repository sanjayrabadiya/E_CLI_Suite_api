using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;


namespace GSC.Respository.SupplyManagement
{
    public interface IProductVerificationDetailRepository : IGenericRepository<ProductVerificationDetail>
    {
        ProductVerificationDetailDto GetProductVerificationDetailList(int ProductReceiptId);
    }
}
