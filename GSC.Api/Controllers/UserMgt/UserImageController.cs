using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class UserImageController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IUserImageRepository _userImageRepository;

        public UserImageController(IUserImageRepository userImageRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper)
        {
            _userImageRepository = userImageRepository;
            _uow = uow;
            _mapper = mapper;
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var userImage = _userImageRepository.Find(id);
            var userImageDto = _mapper.Map<UserImageDto>(userImage);
            return Ok(userImageDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] UserImageDto userImageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            userImageDto.Id = 0;
            var userImage = _mapper.Map<UserImage>(userImageDto);
            _userImageRepository.Add(userImage);
            if (_uow.Save() <= 0) throw new Exception("Creating User Image failed on save.");
            return Ok(userImage.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] UserImageDto userImageDto)
        {
            if (userImageDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var userImage = _mapper.Map<UserImage>(userImageDto);
            _userImageRepository.Update(userImage);
            if (_uow.Save() <= 0) throw new Exception("Updating User Image failed on save.");
            return Ok(userImage.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            var userImage = _userImageRepository.Find(id);
            _userImageRepository.Delete(userImage);
            _uow.Save();
            return Ok();
        }
    }
}