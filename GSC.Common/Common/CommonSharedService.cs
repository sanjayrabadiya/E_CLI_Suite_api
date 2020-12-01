using GSC.Shared.Caching;
using GSC.Shared.JWTAuth;

namespace GSC.Common.Common
{
    public interface ICommonSharedService
    {
        IJwtTokenAccesser JwtTokenAccesser { get; }
        IAuditTracker AuditTracker { get; }
        IGSCCaching GSCCaching { get; }
    }

    public class CommonSharedService : ICommonSharedService
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IAuditTracker _auditTracker;
        private readonly  IGSCCaching _gSCCaching;
        public CommonSharedService(IJwtTokenAccesser jwtTokenAccesser, IAuditTracker auditTracker, IGSCCaching gSCCaching)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _auditTracker = auditTracker;
            _gSCCaching = gSCCaching;
        }
        public IJwtTokenAccesser JwtTokenAccesser => _jwtTokenAccesser;
        public IAuditTracker AuditTracker => _auditTracker;
        public IGSCCaching GSCCaching => _gSCCaching;
    }

}
