using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface ILettersActivityRepository : IGenericRepository<LettersActivity>
    {
        List<DropDownDto> GetActivityTypeDropDown();
        List<DropDownDto> GetLettersFormatTypeDropDown();
        List<DropDownDto> GetMedicalUserTypeDown(int siteId);
        List<LettersActivityDateDropDown> getSelectDateDrop(int projectId, int siteId);
        List<LettersActivityGridDto> GetLettersActivityList(bool isDeleted, int? projectId);
        void CreateLettersEmail(LettersFormate lettersFormate, LettersActivityDto lettersActivityDto);
        void updateLettersEmail(LettersActivityDto lettersActivityDto);
        List<LettersActivityDto> UserRoles(int ProjectId);
        string GetSendMail(SendMailModel sendMailModel);
    }
}