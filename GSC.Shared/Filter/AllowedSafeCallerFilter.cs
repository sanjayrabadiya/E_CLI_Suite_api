using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace GSC.Shared.Filter
{
    public class AllowedSafeCallerFilter : ActionFilterAttribute
    {
        private readonly IOptions<SafeIPAddress> _ipaddresses;

        public AllowedSafeCallerFilter(IOptions<SafeIPAddress> ipaddresses)
        {
            _ipaddresses = ipaddresses;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!_ipaddresses.Value.IpList.Any())
                base.OnActionExecuting(context);

            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
            var badIp = true;
            foreach (var address in _ipaddresses.Value.IpList)
            {
                if (remoteIp.IsIPv4MappedToIPv6) remoteIp = remoteIp.MapToIPv4();
                var testIp = IPAddress.Parse(address);
                if (testIp.Equals(remoteIp))
                {
                    badIp = false;
                    break;
                }
            }

            if (badIp)
            {
                context.Result = new BadRequestResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}