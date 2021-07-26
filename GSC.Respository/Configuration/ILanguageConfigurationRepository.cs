using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Configuration
{
   public interface ILanguageConfigurationRepository : IGenericRepository<LanguageConfiguration>
    {
        List<LanguageConfigurationGridDto> GetlanguageConfiguration(bool isDeleted);
        string Duplicate(LanguageConfiguration objSave);
        string DuplicateLanguage(LanguageConfigurationDetails objSave);
        List<LanguageConfigurationDetailsGridDto> GetLanguageDetails(int LanguageConfigurationDetailsId);
    }
}
