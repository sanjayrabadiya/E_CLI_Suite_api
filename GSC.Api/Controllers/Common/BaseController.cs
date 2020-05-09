using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Common
{
    [Authorize]
    public class BaseController : Controller
    {
    }
}