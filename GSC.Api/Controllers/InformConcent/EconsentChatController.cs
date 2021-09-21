using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Api.Controllers.Common;
using GSC.Api.Hubs;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.InformConcent;
using GSC.Respository.EmailSender;
using GSC.Respository.InformConcent;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentChatController : BaseController
    {
        // note: messagehub not accessible in repository so all the messagehub related logic written in this class
        private readonly IUnitOfWork _uow;
        private readonly IEconsentChatRepository _econsentChatRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IHubContext<MessageHub> _hubcontext;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;

        public EconsentChatController(IUnitOfWork uow,
                                        IJwtTokenAccesser jwtTokenAccesser,
                                        IHubContext<MessageHub> hubcontext,
                                        IEconsentChatRepository econsentChatRepository,IUserRepository userRepository,IEmailSenderRespository emailSenderRespository)
        {
            _econsentChatRepository = econsentChatRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _hubcontext = hubcontext;
            _userRepository = userRepository;
            _emailSenderRespository = emailSenderRespository;
        }

        [HttpGet]
        [Route("GetChatUsersList")]
        public IActionResult GetChatUsersList()
        {
            // display users for chat
            var users = _econsentChatRepository.GetChatUsersList();
            return Ok(users);
        }

        [HttpGet]
        [Route("GetEconsentChat/{userid}")]
        public IActionResult GetEconsentChat(int userid)
        {
            // display messages of selected user
            var data = _econsentChatRepository.GetEconsentChat(userid);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetUnReadMessagecount")]
        public IActionResult GetUnReadMessagecount()
        {
            // unread message count
            var data = _econsentChatRepository.GetUnReadMessagecount();
            return Ok(data);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EconsentChat econsentChat)
        {
            // insert message details in econsentchat table          
            econsentChat.Salt= Cryptography.CreateSaltKey();
            econsentChat.Message = EncryptionDecryption.EncryptString(econsentChat.Salt, econsentChat.Message);         
            _econsentChatRepository.Add(econsentChat);
            _uow.Save();           
            econsentChat.Message = EncryptionDecryption.DecryptString(econsentChat.Salt, econsentChat.Message);
            // var senderdetails = _userRepository.Find(econsentChat.ReceiverId);
            //var connection = ConnectedUser.Ids.Where(x => x.userId == econsentChat.ReceiverId).Any();
            //if (!senderdetails.IsLogin)
            //{
            //    _emailSenderRespository.SendOfflineChatNotification(senderdetails.Email, senderdetails.FirstName);
            //}
            return Ok(econsentChat);
        }

        [HttpGet]
        [Route("DeliverFlagUpdate/{messageId}")]
        public IActionResult DeliverFlagUpdate(int messageId)
        {
            // delivered flag update when message delivered
            var econsentChat = _econsentChatRepository.Find(messageId);
            econsentChat.IsDelivered = true;
            econsentChat.DeliveredDateTime = _jwtTokenAccesser.GetClientDate();
            _econsentChatRepository.Update(econsentChat);
            _uow.Save();
            econsentChat.Message = EncryptionDecryption.DecryptString(econsentChat.Salt, econsentChat.Message);
            return Ok(econsentChat);
        }

        [HttpPut]
        [Route("AllMessageDelivered/{receiverId}")]
        public async Task<IActionResult> AllMessageDelivered(int receiverId)
        {
            // this method calls when user login then update delivered flag for all the unsend messages
            var messages = _econsentChatRepository.FindBy(x => x.ReceiverId == receiverId && x.IsDelivered == false).ToList();
            for (int i = 0; i < messages.Count; i++)
            {
                messages[i].IsDelivered = true;
                messages[i].DeliveredDateTime = _jwtTokenAccesser.GetClientDate();
                _econsentChatRepository.Update(messages[i]);
            }
            _uow.Save();
            List<int> senderids = new List<int>();
            senderids = messages.Select(x => x.SenderId).Distinct().ToList();
            var userlist = ConnectedUser.Ids.Where(x => senderids.Contains(x.userId)).ToList();
            List<string> listData = new List<string>();
            for (int i = 0; i < userlist.Count; i++)
            {
                listData.Add(userlist[i].connectionId);
            }
            IReadOnlyList<string> readOnlyData = listData.AsReadOnly();
            await _hubcontext.Clients.Clients(readOnlyData).SendAsync("AllMessageDelivered", receiverId);
            return Ok();
        }

        [HttpPut]
        [Route("AllMessageRead/{senderId}")]
        public async Task<IActionResult> AllMessageRead(int senderId)
        {
            //all message read flag update when user clicks on particular user chat
            _econsentChatRepository.AllMessageRead(senderId);
            var connection = ConnectedUser.Ids.Where(x => x.userId == senderId).ToList().FirstOrDefault();
            if (connection != null)
            {
                var connectionId = connection.connectionId;
                await _hubcontext.Clients.Client(connectionId).SendAsync("AllMessageRead", _jwtTokenAccesser.UserId);
            }
            return Ok();
        }
    }
}

