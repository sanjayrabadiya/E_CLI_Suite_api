using GSC.Common.GenericRespository;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Shared.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.Extensions.Options;
using OpenTokSDK;
using OpenTokSDK.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            OpenTok opentok = new OpenTok(Convert.ToInt32(_VideoAPISettings.Value.API_KEY), _VideoAPISettings.Value.API_SECRET);
            string sessionId = opentok.CreateSession("",MediaMode.ROUTED, ArchiveMode.ALWAYS).Id;
            string token = opentok.GenerateToken(sessionId,Role.MODERATOR);
            string token2 = opentok.GenerateToken(sessionId, Role.MODERATOR);
            EConsentVideoDto eConsentVideoDto = new EConsentVideoDto();
            eConsentVideoDto.SessionId = sessionId;
            eConsentVideoDto.Publishertoken = token;
            eConsentVideoDto.Subscribertoken = token2;
            eConsentVideoDto.ApiKey = _VideoAPISettings.Value.API_KEY;
            eConsentVideoDto.SenderUserId = _jwtTokenAccesser.UserId;
            eConsentVideoDto.ReceiverUserId = ReceiverUserId;
            eConsentVideoDto.RequestDelivered = false;
            return eConsentVideoDto;
        }
    }
}
