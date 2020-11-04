using GSC.Data.Dto.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EConsentUserChatDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string LastMessage { get; set; }
        public int UnReadMsgCount { get; set; }
        public bool IsLogin { get; set; }
        public string LastMessageStatus { get; set; }
    }
}
