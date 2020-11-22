using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Respository.Screening;
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningTemplateValueCommentController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IScreeningTemplateValueCommentRepository _screeningTemplateValueCommentRepository;
        private readonly IUnitOfWork _uow;

        public ScreeningTemplateValueCommentController(
            IScreeningTemplateValueCommentRepository screeningTemplateValueCommentRepository,
            IUnitOfWork uow, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser)
        {
            _screeningTemplateValueCommentRepository = screeningTemplateValueCommentRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{screeningTemplateValueId}")]
        public IActionResult Get(int screeningTemplateValueId)
        {
            if (screeningTemplateValueId <= 0) return BadRequest();

            var commentsDto = _screeningTemplateValueCommentRepository.GetComments(screeningTemplateValueId);

            return Ok(commentsDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ScreeningTemplateValueCommentDto screeningTemplateValueCommentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var screeningTemplateValueComment =
                _mapper.Map<ScreeningTemplateValueComment>(screeningTemplateValueCommentDto);

            screeningTemplateValueComment.Id = 0;
            screeningTemplateValueComment.RoleId = _jwtTokenAccesser.RoleId;

            _screeningTemplateValueCommentRepository.Add(screeningTemplateValueComment);

            if (_uow.Save() <= 0) throw new Exception("Creating Screening Entry failed on save.");

            return Ok(screeningTemplateValueComment.Id);
        }
    }
}