using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Helper;
using GSC.Respository.Volunteer;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerQueryController : BaseController
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IVolunteerQueryRepository _volunteerQueryRepository;
        private readonly IUnitOfWork _uow;

        public VolunteerQueryController(
            IMapper mapper
            , IJwtTokenAccesser jwtTokenAccesser
            , IVolunteerRepository volunteerRepository
            , IVolunteerQueryRepository volunteerQueryRepository
            , IUnitOfWork uow)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _volunteerRepository = volunteerRepository;
            _volunteerQueryRepository = volunteerQueryRepository;
            _uow = uow;
        }

        [HttpPost]
        [Route("Search")]
        public IActionResult Search([FromBody] VolunteerSearchDto search)
        {
            var volunteers = _volunteerRepository.Search(search);
            return Ok(volunteers);
        }

        [HttpPost]
        [Route("QuerySearch")]
        public IActionResult QuerySearch([FromBody] VolunteerQuerySearchDto search)
        {
            var volunteers = _volunteerQueryRepository.VolunteerQuerySearch(search);
            return Ok(volunteers);
        }


        [HttpGet("AutoCompleteSearch")]
        public IActionResult AutoCompleteSearch(string searchText)
        {
            var result = _volunteerRepository.QueryAutoCompleteSearch(searchText, true);
            return Ok(result);
        }

        [HttpPost("AddQuery")]
        public IActionResult AddQuery([FromBody] VolunteerQueryDto volunteerQueryCommentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var volunteerQueryValue = _volunteerQueryRepository.GetLatest(volunteerQueryCommentDto.VolunteerId, volunteerQueryCommentDto.FieldName);

            if (volunteerQueryValue == null)
                volunteerQueryCommentDto.QueryStatus = CommentStatus.Open;
            else if (volunteerQueryValue.QueryStatus == CommentStatus.Open && volunteerQueryCommentDto.ReasonId == 0)
                return BadRequest("Query already open for this field.");


            else
            {
                if (volunteerQueryValue.QueryStatus == CommentStatus.Open && volunteerQueryValue.UserRole == _jwtTokenAccesser.RoleId)
                    volunteerQueryCommentDto.QueryStatus = CommentStatus.Closed;
                else if (volunteerQueryValue.QueryStatus == CommentStatus.Open && volunteerQueryValue.UserRole != _jwtTokenAccesser.RoleId && volunteerQueryCommentDto.IsDriect == true)
                    volunteerQueryCommentDto.QueryStatus = CommentStatus.Answered;
                else if (volunteerQueryValue.QueryStatus == CommentStatus.Open && volunteerQueryValue.UserRole != _jwtTokenAccesser.RoleId)
                    volunteerQueryCommentDto.QueryStatus = CommentStatus.Resolved;
                else if (volunteerQueryValue.QueryStatus == CommentStatus.Answered || volunteerQueryValue.QueryStatus == CommentStatus.Resolved)
                    volunteerQueryCommentDto.QueryStatus = CommentStatus.Closed;
                else if (volunteerQueryValue.QueryStatus == CommentStatus.Closed)
                    volunteerQueryCommentDto.QueryStatus = CommentStatus.Open;
                else
                    volunteerQueryCommentDto.QueryStatus = CommentStatus.Answered;
            }
            var volunteerQueryComment = _mapper.Map<VolunteerQuery>(volunteerQueryCommentDto);
            volunteerQueryComment.UserRole = _jwtTokenAccesser.RoleId;
            _volunteerQueryRepository.Add(volunteerQueryComment);

            if (_uow.Save() <= 0)
                throw new Exception("Creating Value Query failed on save.");

            return Ok(volunteerQueryComment.Id);
        }


        [HttpGet]
        [Route("GetDetails/{VolunteerId}")]
        public IActionResult GetDetails(int VolunteerId)
        {
            var comments = _volunteerQueryRepository.GetData(VolunteerId);
            return Ok(comments);
        }

        [HttpGet]
        [Route("GetDetailsByVolunteerId/{VolunteerId}")]
        public IActionResult GetDetailsByVolunteerId(int VolunteerId)
        {
            var queries = _volunteerQueryRepository.GetDetailsByVolunteerId(VolunteerId);
            return Ok(queries);
        }

    }
}
