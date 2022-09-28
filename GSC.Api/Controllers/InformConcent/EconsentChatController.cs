﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Hubs;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Respository.EmailSender;
using GSC.Respository.InformConcent;
using GSC.Respository.UserMgt;
using GSC.Shared.Configuration;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

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
       // private readonly IHubContext<MessageHub> _hubcontext;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly ICentreUserService _centreUserService;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly IMapper _mapper;

        public EconsentChatController(IUnitOfWork uow,
                                        IJwtTokenAccesser jwtTokenAccesser,
                                        ICentreUserService centreUserService,
                                         //  IHubContext<MessageHub> hubcontext,
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
            if (ConnectedUser.Ids != null)
            {
                var userlist = ConnectedUser.Ids.Where(x => senderids.Contains(x.userId)).ToList();
                List<string> listData = new List<string>();
                for (int i = 0; i < userlist.Count; i++)
                {
                    listData.Add(userlist[i].connectionId);
                }
                IReadOnlyList<string> readOnlyData = listData.AsReadOnly();

                EconsentChatCentralDto obj = new EconsentChatCentralDto();
                obj.ReceiverId = receiverId;
                obj.ReadOnlyData = readOnlyData;

                var result = await _centreUserService.AllMessageDelivered($"{_environmentSetting.Value.CentralApi}Chat/AllMessageDelivered", obj);
                //   await _hubcontext.Clients.Clients(readOnlyData).SendAsync("AllMessageDelivered", receiverId);
            }
            return Ok();
        }

        [HttpPut]
        [Route("AllMessageRead/{senderId}")]
        public async Task<IActionResult> AllMessageRead(int senderId)
        {
            //all message read flag update when user clicks on particular user chat
            _econsentChatRepository.AllMessageRead(senderId);
            
            await _centreUserService.AllMessageRead($"{_environmentSetting.Value.CentralApi}Chat/AllMessageRead/{_jwtTokenAccesser.UserId}/{senderId}");
            
            return Ok();
        }
    }
}

