using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode.Generate;
using GSC.Data.Entities.Barcode.Generate;

namespace GSC.Respository.Barcode.Generate
{
    public interface IBarcodeGenerateRepository : IGenericRepository<BarcodeGenerate>
    {
        List<BarcodeGenerateDto> GetBarcodeGenerate(bool isDeleted);
        Task<List<BarcodeGenerateDto>> GetGenerateBarcodeDetail(int[] templateId);
    }
}