using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;

namespace GSC.Respository.Volunteer
{
    public interface IVolunteerLanguageRepository : IGenericRepository<VolunteerLanguage>
    {
        List<VolunteerLanguageDto> GetLanguages(int volunteerId,bool isDelete);
        void RemoveExisting(int id, int volunteerId, int languageId);
    }
}