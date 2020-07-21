﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ETMFWorkplaceController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IETMFWorkplaceRepository _eTMFWorkplaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ETMFWorkplaceController(IProjectRepository projectRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            ICountryRepository countryRepository,
              IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
              IUploadSettingRepository uploadSettingRepository
            )
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _uow = uow;
            _mapper = mapper;
            _eTMFWorkplaceRepository = eTMFWorkplaceRepository;
            _countryRepository = countryRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        [Route("Get")]
        [HttpGet]
        public IActionResult Get(bool isDeleted)
        {
            //var projectworkplace = _eTMFWorkplaceRepository.Get(1);

            var projects = _eTMFWorkplaceRepository.FindByInclude(x => x.DeletedBy == null, x => x.Project);
            var projectsDto = _mapper.Map<IEnumerable<ETMFWorkplaceDto>>(projects).ToList();

            projectsDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(projectsDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var projectworkplace = _eTMFWorkplaceRepository.All.Include(x => x.Project);
            return Ok(projectworkplace);
        }

        [Route("GetTreeview/{projectId}")]
        [HttpGet]
        public IActionResult GetTreeview(int projectId)
        {
            var projectworkplace = _eTMFWorkplaceRepository.GetTreeview(projectId);
            return Ok(projectworkplace);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ETMFWorkplaceDto eTMFWorkplaceDto)
        {

            var validate = _eTMFWorkplaceRepository.Duplicate(eTMFWorkplaceDto.ProjectId);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            var projectDetail = _projectRepository.Find(eTMFWorkplaceDto.ProjectId);
            var childProjectList = _projectRepository.GetChildProjectDropDown(eTMFWorkplaceDto.ProjectId);
            var countryList = _countryRepository.GetCountryByProjectIdDropDown(eTMFWorkplaceDto.ProjectId);
            var artificiteList = _etmfArtificateMasterLbraryRepository.GetArtifcateWithAllList();
            var imageUrl = _uploadSettingRepository.GetDocumentPath();

            var SaveFolderStructure = _eTMFWorkplaceRepository.SaveFolderStructure(projectDetail, childProjectList, countryList, artificiteList, imageUrl);

            _eTMFWorkplaceRepository.Add(SaveFolderStructure);
            if (_uow.Save() <= 0) throw new Exception("Creating ETMFWorkplace failed on save.");
            return Ok(SaveFolderStructure.Id);
        }

        [HttpGet]
        [Route("GetCountryByParentProjectIdDropDown/{ParentProjectId}")]
        public IActionResult GetDrugDropDown(int ParentProjectId)
        {
            return Ok(_countryRepository.GetCountryByProjectIdDropDown(ParentProjectId));
        }

        [HttpGet]
        [Route("GetChildProjectDropDown/{ParentProjectId}")]
        public IActionResult GetChildProjectDropDown(int ParentProjectId)
        {
            return Ok(_projectRepository.GetChildProjectDropDown(ParentProjectId));
        }
    }
}