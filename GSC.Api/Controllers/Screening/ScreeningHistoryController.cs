using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Respository.Common;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningHistoryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IScreeningHistoryRepository _screeningHistoryRepository;
        private readonly IUnitOfWork<GscContext> _uow;

        public ScreeningHistoryController(IScreeningHistoryRepository screeningHistoryRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IUserRecentItemRepository userRecentItemRepository)
        {
            _screeningHistoryRepository = screeningHistoryRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{volunteerId}/{lastDay}")]
        public IActionResult Get(int volunteerId, int lastDay)
        {
            if (volunteerId <= 0) return BadRequest();

            return Ok(_screeningHistoryRepository.GetScreeningHistoryByVolunteerId(volunteerId, lastDay));
        }

        [HttpPut]
        public IActionResult Put([FromBody] ScreeningHistoryDto screeningHistoryDto)
        {
            if (screeningHistoryDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var screeningHistory = _mapper.Map<ScreeningHistory>(screeningHistoryDto);
            screeningHistory.Id = screeningHistoryDto.Id;
            _screeningHistoryRepository.Update(screeningHistory);
            if (_uow.Save() <= 0) throw new Exception("Updating screening history failed on update.");
            return Ok(screeningHistory.Id);
        }
    }
}