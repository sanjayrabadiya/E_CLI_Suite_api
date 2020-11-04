using GSC.Common.GenericRespository;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public interface IEconsentChatRepository : IGenericRepository<EconsentChat>
    {
        List<EConsentUserChatDto> GetChatUsersList();

        List<EconsentChat> GetEconsentChat(int userId);
        int GetUnReadMessagecount();
        
    }
}
