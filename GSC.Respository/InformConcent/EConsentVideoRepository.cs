using GSC.Common.GenericRespository;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Shared.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.Extensions.Options;
using AgoraIO.Rtm;
using AgoraIO.Media;
using System;

namespace GSC.Respository.InformConcent
{
    public class EConsentVideoRepository : GenericRespository<EConsentVideo>, IEConsentVideoRepository
    {
        private readonly IOptions<VideoAPISettings> _VideoAPISettings;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public EConsentVideoRepository(IOptions<VideoAPISettings> VideoAPISettings,
                                    IGSCContext context,
                                    IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _VideoAPISettings = VideoAPISettings;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        public EConsentVideoDto GenerateVideoSessionandToken(int ReceiverUserId)
        {
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //OpenTok opentok = new OpenTok(Convert.ToInt32(_VideoAPISettings.Value.API_KEY), _VideoAPISettings.Value.API_SECRET);
            //string sessionId = opentok.CreateSession("",MediaMode.ROUTED, ArchiveMode.ALWAYS).Id;
            //string token = opentok.GenerateToken(sessionId);
            //string token2 = opentok.GenerateToken(sessionId);
            //uint privilegeExpiredTs =
            string channelName = $"CHANNEL-{_jwtTokenAccesser.UserId}";
            uint _expireTimeInSeconds = 3600;
            uint privilegeExpiredTs = _expireTimeInSeconds + (uint)Utils.getTimestamp();
            string token1 = RtcTokenBuilder.buildTokenWithUID(_VideoAPISettings.Value.APP_ID, _VideoAPISettings.Value.APP_CERTIFICATE, channelName, (uint)_jwtTokenAccesser.UserId, RtcTokenBuilder.Role.RolePublisher, privilegeExpiredTs);
            string token2 = RtcTokenBuilder.buildTokenWithUID(_VideoAPISettings.Value.APP_ID, _VideoAPISettings.Value.APP_CERTIFICATE, channelName, (uint)ReceiverUserId, RtcTokenBuilder.Role.RoleSubscriber, privilegeExpiredTs);
            EConsentVideoDto eConsentVideoDto = new EConsentVideoDto();
            eConsentVideoDto.SessionId = channelName;
            eConsentVideoDto.Publishertoken = token1;
            eConsentVideoDto.Subscribertoken = token2;
            eConsentVideoDto.ApiKey = "NA";
            eConsentVideoDto.SenderUserId = _jwtTokenAccesser.UserId;
            eConsentVideoDto.ReceiverUserId = ReceiverUserId;
            eConsentVideoDto.RequestDelivered = false;
            return eConsentVideoDto;
        }
    }
}
