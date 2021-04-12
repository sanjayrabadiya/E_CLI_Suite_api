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
            var data = _EConsentVideoRepository.GenerateVideoSessionandToken(ReceiverUserId);
            var eConsentVideo = _mapper.Map<EConsentVideo>(data);
            _EConsentVideoRepository.Add(eConsentVideo);
            _uow.Save();
            
            //await _hubContext.Clients.Client(connectionId).SendAsync("SessionandTokenGeneratedforVideo", data);
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
            var eConsentVideo = _EConsentVideoRepository.Find(id);
            eConsentVideo.CallStatus = VideoCallStatus.CallAccepted;
            eConsentVideo.CallStartTime = DateTime.Now;
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();
            return Ok();
        }

        [HttpPut]
        [Route("CallEndsuccessfullyafterconnecting/{id}")]
        public async Task<IActionResult> CallEndsuccessfullyafterconnecting(int id)
        {
            var eConsentVideo = _EConsentVideoRepository.Find(id);
            eConsentVideo.EndCallBy = (eConsentVideo.SenderUserId == _jwtTokenAccesser.UserId) ? VideoCallStatusCallEndBy.Sender : VideoCallStatusCallEndBy.Receiver;
            eConsentVideo.CallEndTime = DateTime.Now;
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
