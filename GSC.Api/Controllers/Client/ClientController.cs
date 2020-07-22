using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Client;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Client;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Client
{
    [Route("api/[controller]")]
    public class ClientController : BaseController
    {
        private readonly IClientRepository _clientRepository;
        private readonly IClientTypeRepository _clientTypeRepository;
        private readonly IRoleRepository _securityRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public ClientController(IClientRepository clientRepository,
            IClientTypeRepository clientTypeRepository,
            IRoleRepository securityRoleRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IUploadSettingRepository uploadSettingRepository)
        {
            _clientTypeRepository = clientTypeRepository;
            _securityRoleRepository = securityRoleRepository;
            _clientRepository = clientRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var clients = _clientRepository.All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var clientsDto = _mapper.Map<IEnumerable<ClientDto>>(clients);
            var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            clientsDto.ForEach(b =>
            {
                b.LogoPath = imageUrl + (b.Logo ?? DocumentService.DefulatLogo);
                b.FirstName = _userRepository.Find((int)b.UserId).FirstName +" "+ _userRepository.Find((int)b.UserId).LastName;
                b.LastName = _userRepository.Find((int)b.UserId).LastName;
                b.RoleName = _securityRoleRepository.Find((int)b.RoleId).RoleName;
                b.ClientTypeName = _clientTypeRepository.Find(b.ClientTypeId).ClientTypeName;
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(clientsDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var client = _clientRepository.Find(id);
            var clientDto = _mapper.Map<ClientDto>(client);
            var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            clientDto.LogoPath = imageUrl + (clientDto.Logo ?? DocumentService.DefulatLogo);
            return Ok(clientDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ClientDto clientDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            clientDto.Id = 0;

            if (clientDto.FileModel?.Base64?.Length > 0)
                clientDto.Logo = new ImageService().ImageSave(clientDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.Logo);


            var client = _mapper.Map<Data.Entities.Client.Client>(clientDto);
            var validate = _clientRepository.DuplicateClient(client);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _clientRepository.Add(client);
            if (_uow.Save() <= 0) throw new Exception("Creating client failed on save.");
            _mapper.Map<ClientDto>(client);
            return Ok(client.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ClientDto clientDto)
        {
            if (clientDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (clientDto.FileModel?.Base64?.Length > 0)
                clientDto.Logo = new ImageService().ImageSave(clientDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.Logo);

            var client = _mapper.Map<Data.Entities.Client.Client>(clientDto);

            var validate = _clientRepository.DuplicateClient(client);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _clientRepository.AddOrUpdate(client);

            if (_uow.Save() <= 0) throw new Exception("Updating client failed on save.");

            return Ok(client.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _clientRepository.Find(id);

            if (record == null)
                return NotFound();

            _clientRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _clientRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _clientRepository.DuplicateClient(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _clientRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetClientDropDown")]
        public IActionResult GetClientDropDown()
        {
            return Ok(_clientRepository.GetClientDropDown());
        }
    }
}