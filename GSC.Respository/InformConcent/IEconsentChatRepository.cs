﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.InformConcent
{
    public interface IEconsentChatRepository : IGenericRepository<EconsentChat>
    {
        List<EConsentUserChatDto> GetChatUsersList();
        EconsentChatDetailDto GetEconsentChat(EconcentChatParameterDto details);
        List<EconsentChatDto> GetEconsentChat(int userId);
        int GetUnReadMessagecount();
        void AllMessageRead(int senderId);
    }
}
