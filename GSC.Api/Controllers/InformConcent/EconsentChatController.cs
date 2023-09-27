using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Respository.EmailSender;
using GSC.Respository.FirebaseNotification;
using GSC.Respository.InformConcent;
using GSC.Respository.UserMgt;
using GSC.Shared.Configuration;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentChatController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IEconsentChatRepository _econsentChatRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly ICentreUserService _centreUserService;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly IMapper _mapper;
        private readonly IFirebaseNotification _firebaseNotification;

        public EconsentChatController(IUnitOfWork uow,
                                        IJwtTokenAccesser jwtTokenAccesser,
                                        ICentreUserService centreUserService,
                                        IFirebaseNotification firebaseNotification,
                                         IOptions<EnvironmentSetting> environmentSetting,
                                        IEconsentChatRepository econsentChatRepository, IUserRepository userRepository, IEmailSenderRespository emailSenderRespository,
                                        IMapper mapper)
        {
            _econsentChatRepository = econsentChatRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _centreUserService = centreUserService;
            //_hubcontext = hubcontext;
            _userRepository = userRepository;
            _emailSenderRespository = emailSenderRespository;
            _environmentSetting = environmentSetting;
            _mapper = mapper;
            _firebaseNotification = firebaseNotification;
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

        
        [HttpPost]
        [Route("GetEconsentChat")]
        public IActionResult GetEconsentChat([FromBody] EconcentChatParameterDto details)
        {
            // display messages of selected user
            var data = _econsentChatRepository.GetEconsentChat(details);
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
            econsentChat.Salt = Cryptography.CreateSaltKey();
            econsentChat.SendDateTime = _jwtTokenAccesser.GetClientDate();
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
            econsentChat.Sender = _userRepository.Find(econsentChat.SenderId);
            econsentChat.Receiver = _userRepository.Find(econsentChat.ReceiverId);
            _firebaseNotification.SendEConsentChatMessage(econsentChat);
            var result = _mapper.Map<EconsentChatDto>(econsentChat);
            return Ok(result);
        }

        [HttpGet]
        [Route("DeliverFlagUpdate/{messageId}")]
        public IActionResult DeliverFlagUpdate(int messageId)
        {
            // delivered flag update when message delivered
            var econsentChat = _econsentChatRepository.Find(messageId);
            if (econsentChat == null)
                return Ok();
            econsentChat.IsDelivered = true;
            econsentChat.DeliveredDateTime = _jwtTokenAccesser.GetClientDate();
            _econsentChatRepository.Update(econsentChat);
            _uow.Save();
            var result = _mapper.Map<EconsentChatDto>(econsentChat);
            econsentChat.Message = EncryptionDecryption.DecryptString(econsentChat.Salt, econsentChat.Message);
            return Ok(result);
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

            EconsentChatCentralDto obj = new EconsentChatCentralDto();
            obj.ReceiverId = receiverId;
            obj.SenderIds = senderids;
            return Ok(obj);
        }

        [HttpPut]
        [Route("AllMessageRead/{senderId}")]
        public async Task<IActionResult> AllMessageRead(int senderId)
        {
            //all message read flag update when user clicks on particular user chat
            _econsentChatRepository.AllMessageRead(senderId);
            
            //await _centreUserService.AllMessageRead($"{_environmentSetting.Value.CentralApi}Chat/AllMessageRead/{_jwtTokenAccesser.UserId}/{senderId}");
            
            return Ok(senderId);
        }
    }
}

