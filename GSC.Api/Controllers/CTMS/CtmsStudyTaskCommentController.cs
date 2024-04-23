using GSC.Api.Controllers.Common;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsStudyTaskCommentController : BaseController
    {
        private readonly ICtmsStudyPlanTaskCommentRepository _ctmsStudyPlanTaskCommentRepository;
        public CtmsStudyTaskCommentController(ICtmsStudyPlanTaskCommentRepository ctmsStudyPlanTaskCommentRepository)
        {
            _ctmsStudyPlanTaskCommentRepository = ctmsStudyPlanTaskCommentRepository;
        }


    }
}
