using GSC.Common.GenericRespository;
using GSC.Data.Entities.Configuration;

namespace GSC.Respository.Configuration
{
    public interface INumberFormatRepository : IGenericRepository<NumberFormat>
    {
        string GenerateNumber(string keyName);
        string GetNumberFormat(string keyName, int number);
    }
}