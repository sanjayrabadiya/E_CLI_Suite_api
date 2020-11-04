using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Hubs;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.InformConcent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentChatController : ControllerBase
    {
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IMapper _mapper;
        private readonly IEconsentChatRepository _econsentChatRepository;
        private readonly GscContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IHubContext<MessageHub> _hubcontext;

        public EconsentChatController(IUnitOfWork<GscContext> uow,
                                        IMapper mapper,
                                        IJwtTokenAccesser jwtTokenAccesser,
                                        IHubContext<MessageHub> hubcontext,
                                        IEconsentChatRepository econsentChatRepository)
        {
            _econsentChatRepository = econsentChatRepository;
            _uow = uow;
            _mapper = mapper;
            _context = uow.Context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _hubcontext = hubcontext;
        }

        [HttpGet]
        [Route("GetChatUsersList")]
        public IActionResult GetChatUsersList()
        {
            var users = _econsentChatRepository.GetChatUsersList();
            return Ok(users);
        }

        [HttpGet]
        [Route("GetEconsentChat/{userid}")]
        public IActionResult GetChatUsersList(int userid)
        {
            var data = _econsentChatRepository.GetEconsentChat(userid);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetUnReadMessagecount")]
        public IActionResult GetUnReadMessagecount()
        {
            var data = _econsentChatRepository.GetUnReadMessagecount();
            return Ok(data);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EconsentChat econsentChat)
        {
            _econsentChatRepository.Add(econsentChat);
            _uow.Save();
            return Ok(econsentChat);
        }

        [HttpPut]
        [Route("DeliverFlagUpdate")]
        public IActionResult DeliverFlagUpdate([FromBody] EconsentChat econsentChat)
        {
            econsentChat.IsDelivered = true;
            econsentChat.DeliveredDateTime = DateTime.Now;
            _econsentChatRepository.Update(econsentChat);
            _uow.Save();
            return Ok(econsentChat);
        }

        [HttpPut]
        [Route("AllMessageDelivered/{receiverId}")]
        public async Task<IActionResult> AllMessageDelivered(int receiverId)
        {
            var messages = _econsentChatRepository.FindBy(x => x.ReceiverId == receiverId && x.IsDelivered == false).ToList();
            for (int i = 0; i < messages.Count; i++)
            {
                messages[i].IsDelivered = true;
                messages[i].DeliveredDateTime = DateTime.Now;
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
            var messages = _econsentChatRepository.FindBy(x => x.SenderId == senderId && x.ReceiverId == _jwtTokenAccesser.UserId && x.IsRead == false).ToList();
            for (int i = 0; i < messages.Count; i++)
            {
                messages[i].IsRead = true;
                messages[i].ReadDateTIme = DateTime.Now;
                if (messages[i].IsDelivered == false)
                {
                    messages[i].IsDelivered = true;
                    messages[i].DeliveredDateTime = DateTime.Now;
                }
                _econsentChatRepository.Update(messages[i]);
            }
            _uow.Save();
            try
            {
                var connectionId = ConnectedUser.Ids.Where(x => x.userId == senderId).ToList().FirstOrDefault().connectionId;
                await _hubcontext.Clients.Client(connectionId).SendAsync("AllMessageRead", _jwtTokenAccesser.UserId);
            } catch(Exception ex)
            {

            }
            return Ok();
        }
    }
}

