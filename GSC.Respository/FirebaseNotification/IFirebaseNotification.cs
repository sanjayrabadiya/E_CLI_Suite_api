using GSC.Data.Entities.InformConcent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.FirebaseNotification
{
    public interface IFirebaseNotification
    {
        void SendEConsentMessage(int receiverId);
        void SendEConsentChatMessage(EconsentChat message);
    }
}
