using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.InformConcent;
using GSC.Respository.Configuration;
using GSC.Respository.InformConcent;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GSC.Data.Entities.InformConcent;
using GSC.Shared.Security;
using GSC.Common.UnitOfWork;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace GSC.Respository.FirebaseNotification
{
    public class FirebaseNotification : IFirebaseNotification
    {
        private readonly IAppSettingRepository _settingRepository;
        private readonly IEconsentChatRepository _econsentChatRepository;
        private readonly HttpClient _httpClient;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork _uow;
        public FirebaseNotification(IAppSettingRepository appSettingRepository,
            IEconsentChatRepository econsentChatRepository,
            HttpClient httpClient,
            IJwtTokenAccesser jwtTokenAccesser,
            IUnitOfWork uow)
        {
            _settingRepository = appSettingRepository;
            _econsentChatRepository = econsentChatRepository;
            _httpClient = httpClient;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
        }


        public async Task<string> SendEConsentChatMessage(EconsentChat message)
        {
            try
            {
                var commonSettiongs = _settingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
                var payload = new Payload()
                {
                    priority = "high",
                    to = message.Receiver.FirebaseToken,
                    MessageType = Helper.FirebaseMsgType.EConsetChat,
                    notification = new Notification()
                    {
                        title = message.Sender.UserName,
                        body = message.Message,
                        icon = ""
                    }
                };

                var sent = await SendFirebaseNotification(payload, commonSettiongs);
                if (sent)
                {
                    return "Message successfully sent";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Message sending failed";
        }

        public async void SendEConsentMessage(int receiverId)
        {
            var messages = _econsentChatRepository.FindByInclude(x => x.ReceiverId == receiverId && x.IsDelivered == false, i => i.Receiver, i => i.Sender).ToList();
            var commonSettiongs = _settingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            foreach (var message in messages)
            {
                var payload = new Payload()
                {
                    priority = "high",
                    to = message.Receiver.FirebaseToken,
                    notification = new Notification()
                    {
                        title = $"New Message - {message.Sender.UserName}",
                        body = message.Message,
                        icon = ""
                    }
                };

                var sent = await SendFirebaseNotification(payload, commonSettiongs);
                if (sent)
                {
                    message.IsDelivered = true;
                    message.DeliveredDateTime = _jwtTokenAccesser.GetClientDate();
                    _econsentChatRepository.Update(message);
                    _uow.Save();
                }
            }
        }
        private async Task<bool> SendFirebaseNotification(Payload payload, GeneralSettingsDto commonSettiongs)
        {
            _httpClient.BaseAddress = new Uri(commonSettiongs.FirebaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={commonSettiongs.FirebaseServerId}");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sender", $"id={commonSettiongs.FirebaseSenderId}");

            var json = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync("/fcm/send", httpContent);
            return result.StatusCode.Equals(HttpStatusCode.OK);
        }
    }
}
