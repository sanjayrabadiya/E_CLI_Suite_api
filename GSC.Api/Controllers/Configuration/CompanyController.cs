using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Common;
using GSC.Respository.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class CompanyController : BaseController
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public CompanyController(ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            ILocationRepository locationRepository,
            IUploadSettingRepository uploadSettingRepository)
        {
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _locationRepository = locationRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var companys = _companyRepository.GetCompanies(isDeleted);
            //var companysDto = _mapper.Map<IEnumerable<CompanyDto>>(companys);
            return Ok(companys);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var company = _companyRepository.FindByInclude(t => t.Id == id, t => t.Location).First();
            var companyDto = _mapper.Map<CompanyDto>(company);
            var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            companyDto.LogoPath = imageUrl + (companyDto.Logo ?? DocumentService.DefulatLogo);
            return Ok(companyDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] CompanyDto companyDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            companyDto.Id = 0;


            if (companyDto.FileModel?.Base64?.Length > 0)
                companyDto.Logo = new ImageService().ImageSave(companyDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.Logo);

            var company = _mapper.Map<Company>(companyDto);
            company.Location = _locationRepository.SaveLocation(companyDto.Location);
            _companyRepository.Add(company);
            if (_uow.Save() <= 0) throw new Exception("Creating Company failed on save.");

            return Ok(company.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CompanyDto companyDto)
        {
            if (companyDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (companyDto.FileModel?.Base64?.Length > 0)
                companyDto.Logo = new ImageService().ImageSave(companyDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.Logo);

            var company = _mapper.Map<Company>(companyDto);
            company.Location = _locationRepository.SaveLocation(companyDto.Location);
            _companyRepository.Update(company);
            if (_uow.Save() <= 0) throw new Exception("Updating Company failed on save.");
            return Ok(company.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _companyRepository.Find(id);

            if (record == null)
                return NotFound();

            _companyRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _companyRepository.Find(id);

            if (record == null)
                return NotFound();
            _companyRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetCompanyDropDown")]
        public IActionResult GetCompanyDropDown()
        {
            return Ok(_companyRepository.GetCompanyDropDown());
        }
    }
}