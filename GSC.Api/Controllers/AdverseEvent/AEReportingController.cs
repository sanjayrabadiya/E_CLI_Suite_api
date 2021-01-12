using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Entities.AdverseEvent;
using GSC.Respository.AdverseEvent;
using GSC.Respository.Attendance;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.AdverseEvent
{
    [Route("api/[controller]")]
    [ApiController]
    public class AEReportingController : ControllerBase
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IAEReportingRepository _iAEReportingRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IUnitOfWork _uow;
        public AEReportingController(IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IAEReportingRepository iAEReportingRepository,
            IRandomizationRepository randomizationRepository,
            IUnitOfWork uow
            )
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _iAEReportingRepository = iAEReportingRepository;
            _randomizationRepository = randomizationRepository;
            _uow = uow;
        }

        [HttpGet("GetAEReportingList")]
        public IActionResult GetAEReportingList()
        {
            var data = _iAEReportingRepository.GetAEReportingList();
            return Ok(data);
        }

        [HttpPost]
        public IActionResult Post([FromBody] AEReportingDto aEReportingDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var aEReporting = _mapper.Map<AEReporting>(aEReportingDto);
            var randomization = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization == null)
            {
                ModelState.AddModelError("Message", "Error to save Adverse Event Reporting.");
                return BadRequest(ModelState);
            }
            aEReporting.RandomizationId = randomization.Id;
            aEReporting.IsReviewedDone = false;
            _iAEReportingRepository.Add(aEReporting);

            if (_uow.Save() <= 0) throw new Exception("Error to save Adverse Event Reporting.");
            return Ok();
        }

    }
}
