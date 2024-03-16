using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Helper;
using GSC.Respository.Configuration;
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
    public class EConsentVideoController : BaseController
    {
        private readonly IEConsentVideoRepository _EConsentVideoRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IAppSettingRepository _appSettingRepository;

        public EConsentVideoController(IEConsentVideoRepository EConsentVideoRepository,
                                        IUnitOfWork uow,
                                        IMapper mapper,
                                        IAppSettingRepository appSettingRepository,
                                        IUserRepository userRepository,
                                        IJwtTokenAccesser jwtTokenAccesser)
        {
            _EConsentVideoRepository = EConsentVideoRepository;
            _uow = uow;
            _mapper = mapper;
            _userRepository = userRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _appSettingRepository = appSettingRepository;
        }

        [HttpGet]
        [Route("GenerateVideoSessionandToken/{ReceiverUserId}")]
        public IActionResult GenerateVideoSessionandToken(int ReceiverUserId)
        {
            // main function for connecting video, we want tken and sessionid for connecting video so this api generates token and sessionid
            var data = _EConsentVideoRepository.GenerateVideoSessionandToken(ReceiverUserId);
            var eConsentVideo = _mapper.Map<EConsentVideo>(data);
            _EConsentVideoRepository.Add(eConsentVideo);
            _uow.Save();
            data.Id = eConsentVideo.Id;
            data.SenderUserName = _userRepository.Find(data.SenderUserId).UserName;
            data.ReceiverUserName = _userRepository.Find(data.ReceiverUserId).UserName;
            return Ok(data);
        }

        [HttpPut]
        [Route("RequestDeliverFlagUpdate/{id}")]
        public async Task<IActionResult> DeliverFlagUpdate(int id)
        {
            //when user1 calls to user2 then if user2 receives call request then this flag is updated
            var commonSettiongs = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            var eConsentVideo = await _EConsentVideoRepository.FindAsync(id);
            eConsentVideo.RequestDelivered = true;
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();
            var eConsentVideoDto = _mapper.Map<EConsentVideoDto>(eConsentVideo);
            eConsentVideoDto.SenderUserName = _userRepository.Find(eConsentVideoDto.SenderUserId).UserName;
            eConsentVideoDto.ReceiverUserName = _userRepository.Find(eConsentVideoDto.ReceiverUserId).UserName;
            eConsentVideoDto.ApiKey = commonSettiongs.AgoraAppId;
            return Ok(eConsentVideoDto);
        }

        [HttpPut]
        [Route("CallDeclined/{id}")]
        public async Task<IActionResult> CallDeclined(int id)
        {
            // when user1 calls to user2 then if user2 decline the call then this method is called
            var eConsentVideo = await _EConsentVideoRepository.FindAsync(id);
            eConsentVideo.CallStatus = VideoCallStatus.CallDeclined;
            eConsentVideo.EndCallBy = VideoCallStatusCallEndBy.Receiver;
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();
            return Ok(eConsentVideo);
        }

        [HttpPut]
        [Route("CallNotAnswered/{id}")]
        public async Task<IActionResult> CallNotAnswered(int id)
        {
            // when user1 calls to user2 then if user2 not answer the call then this method is called
            var eConsentVideo = await _EConsentVideoRepository.FindAsync(id);
            eConsentVideo.CallStatus = VideoCallStatus.NotAnswered;
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();           
            return Ok(eConsentVideo);
        }

        [HttpPut]
        [Route("CallEndBySenderBeforeConnecting/{id}")]
        public async Task<IActionResult> CallEndBySenderBeforeConnecting(int id)
        {
            // when user1 calls to user2 then if user1 ends the call before recived by the user2 then this method is called
            var eConsentVideo = await _EConsentVideoRepository.FindAsync(id);
            eConsentVideo.CallStatus = VideoCallStatus.CallEndBySenderBeforeConnecting;
            eConsentVideo.EndCallBy = VideoCallStatusCallEndBy.Sender;
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();          
            return Ok(eConsentVideo);
        }

        [HttpPut]
        [Route("CallAcceptedUpdate/{id}")]
        public IActionResult CallAcceptedUpdate(int id)
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
            var eConsentVideo = await _EConsentVideoRepository.FindAsync(id);
            eConsentVideo.EndCallBy = (eConsentVideo.SenderUserId == _jwtTokenAccesser.UserId) ? VideoCallStatusCallEndBy.Sender : VideoCallStatusCallEndBy.Receiver;
            eConsentVideo.CallEndTime = _jwtTokenAccesser.GetClientDate();
            _EConsentVideoRepository.Update(eConsentVideo);
            _uow.Save();
            return Ok(eConsentVideo);
        }
    }
}
