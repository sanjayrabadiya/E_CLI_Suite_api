using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Hubs;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Helper;
using GSC.Respository.InformConcent;
using GSC.Respository.UserMgt;
using GSC.Shared.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EConsentVideoController : ControllerBase
    {
        //note: messagehub not accessible in repository so all the messagehub related logic written in this class
        private readonly IEConsentVideoRepository _EConsentVideoRepository;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IOptions<VideoAPISettings> _VideoAPISettings;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public EConsentVideoController(IEConsentVideoRepository EConsentVideoRepository,
                                        IHubContext<MessageHub> hubContext,
                                        IUnitOfWork uow,
                                        IMapper mapper,
                                        IUserRepository userRepository,
                                        IOptions<VideoAPISettings> VideoAPISettings,
                                        IJwtTokenAccesser jwtTokenAccesser)
        {
            _EConsentVideoRepository = EConsentVideoRepository;
            _hubContext = hubContext;
            _uow = uow;
            _mapper = mapper;
            _userRepository = userRepository;
            _VideoAPISettings = VideoAPISettings;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet]
        [Route("GenerateVideoSessionandToken/{ReceiverUserId}")]
        public async Task<IActionResult> GenerateVideoSessionandToken(int ReceiverUserId)
        {
            // main function for connecting video, we want tken and sessionid for connecting video so this api generates token and sessionid
            var data = _EConsentVideoRepository.GenerateVideoSessionandToken(ReceiverUserId);
            var eConsentVideo = _mapper.Map<EConsentVideo>(data);
            _EConsentVideoRepository.Add(eConsentVideo);
            _uow.Save();
            data.Id = eConsentVideo.Id;
            data.SenderUserName = _userRepository.Find(data.SenderUserId).UserName;
            data.ReceiverUserName = _userRepository.Find(data.ReceiverUserId).UserName;
            if (ConnectedUser.Ids.Where(x => x.userId == ReceiverUserId).FirstOrDefault() != null)
            {
                var connectionId = ConnectedUser.Ids.Where(x => x.userId == ReceiverUserId).FirstOrDefault().connectionId;
                await _hubContext.Clients.Client(connectionId).SendAsync("SendVideoCallRequesttoReceiver", data);
            } else
            {
                var connectionId = ConnectedUser.Ids.Where(x => x.userId == data.SenderUserId).FirstOrDefault().connectionId;
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiverNotConnectedforCall", data);
            }
            return Ok(data);
        }

        [HttpPut]
        [Route("RequestDeliverFlagUpdate/{id}")]
        public async Task<IActionResult> DeliverFlagUpdate(int id)
        {
            //when user1 calls to user2 then if user2 receives call request then this flag is updated
            var eConsentVideo = _EConsentVideoRepository.Find(id);
            eConsentVideo.RequestDelivered = true;
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();
            var eConsentVideoDto = _mapper.Map<EConsentVideoDto>(eConsentVideo);
            eConsentVideoDto.SenderUserName = _userRepository.Find(eConsentVideoDto.SenderUserId).UserName;
            eConsentVideoDto.ReceiverUserName = _userRepository.Find(eConsentVideoDto.ReceiverUserId).UserName;
            eConsentVideoDto.ApiKey = _VideoAPISettings.Value.API_KEY;
            if (ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.SenderUserId).ToList().FirstOrDefault() != null)
            {
                var connectionId = ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.SenderUserId).ToList().FirstOrDefault().connectionId;
                await _hubContext.Clients.Client(connectionId).SendAsync("RequestDeliveredNotificationsendBacktoSender", eConsentVideoDto);
            }
            return Ok(eConsentVideoDto);
        }

        [HttpPut]
        [Route("CallDeclined/{id}")]
        public async Task<IActionResult> CallDeclined(int id)
        {
            // when user1 calls to user2 then if user2 decline the call then this method is called
            var eConsentVideo = _EConsentVideoRepository.Find(id);
            eConsentVideo.CallStatus = VideoCallStatus.CallDeclined;
            eConsentVideo.EndCallBy = VideoCallStatusCallEndBy.Receiver;
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();
            if (ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.SenderUserId).ToList().FirstOrDefault() != null)
            {
                var connectionId = ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.SenderUserId).ToList().FirstOrDefault().connectionId;
                await _hubContext.Clients.Client(connectionId).SendAsync("CallDeclinedNotifysendBacktoSender", id);
            }
            return Ok();
        }

        [HttpPut]
        [Route("CallNotAnswered/{id}")]
        public async Task<IActionResult> CallNotAnswered(int id)
        {
            // when user1 calls to user2 then if user2 not answer the call then this method is called
            var eConsentVideo = _EConsentVideoRepository.Find(id);
            eConsentVideo.CallStatus = VideoCallStatus.NotAnswered;
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();
            if (ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.ReceiverUserId).ToList().FirstOrDefault() != null)
            {
                var connectionId = ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.ReceiverUserId).ToList().FirstOrDefault().connectionId;
                await _hubContext.Clients.Client(connectionId).SendAsync("CallNotAnsweredNotifysendBacktoReceiver", id);
            }
            return Ok();
        }

        [HttpPut]
        [Route("CallEndBySenderBeforeConnecting/{id}")]
        public async Task<IActionResult> CallEndBySenderBeforeConnecting(int id)
        {
            // when user1 calls to user2 then if user1 ends the call before recived by the user2 then this method is called
            var eConsentVideo = _EConsentVideoRepository.Find(id);
            eConsentVideo.CallStatus = VideoCallStatus.CallEndBySenderBeforeConnecting;
            eConsentVideo.EndCallBy = VideoCallStatusCallEndBy.Sender;
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();
            if (ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.ReceiverUserId).ToList().FirstOrDefault() != null)
            {
                var connectionId = ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.ReceiverUserId).ToList().FirstOrDefault().connectionId;
                await _hubContext.Clients.Client(connectionId).SendAsync("CallEndBySenderBeforeConnectingNotifytoReceiver", id);
            }
            return Ok();
        }

        [HttpPut]
        [Route("CallAcceptedUpdate/{id}")]
        public  IActionResult CallAcceptedUpdate(int id)
        {
            // when user1 calls to user2 then user2 call accept then this method is called
            var eConsentVideo = _EConsentVideoRepository.Find(id);
            eConsentVideo.CallStatus = VideoCallStatus.CallAccepted;
            eConsentVideo.CallStartTime = _jwtTokenAccesser.GetClientDate();
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();
            return Ok();
        }

        [HttpPut]
        [Route("CallEndsuccessfullyafterconnecting/{id}")]
        public async Task<IActionResult> CallEndsuccessfullyafterconnecting(int id)
        {
            //calls this method when any of the user end the call
            var eConsentVideo = _EConsentVideoRepository.Find(id);
            eConsentVideo.EndCallBy = (eConsentVideo.SenderUserId == _jwtTokenAccesser.UserId) ? VideoCallStatusCallEndBy.Sender : VideoCallStatusCallEndBy.Receiver;
            eConsentVideo.CallEndTime = _jwtTokenAccesser.GetClientDate();
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();
            if (eConsentVideo.EndCallBy == VideoCallStatusCallEndBy.Sender)
            {
                if (ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.ReceiverUserId).ToList().FirstOrDefault() != null)
                {
                    var connectionId = ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.ReceiverUserId).ToList().FirstOrDefault().connectionId;
                    await _hubContext.Clients.Client(connectionId).SendAsync("CallEndsuccessfullyafterconnecting", id);
                }
            } else
            {
                if (ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.SenderUserId).ToList().FirstOrDefault() != null)
                {
                    var connectionId = ConnectedUser.Ids.Where(x => x.userId == eConsentVideo.SenderUserId).ToList().FirstOrDefault().connectionId;
                    await _hubContext.Clients.Client(connectionId).SendAsync("CallEndsuccessfullyafterconnecting", id);
                }
            }
            return Ok();
        }
    }
}
