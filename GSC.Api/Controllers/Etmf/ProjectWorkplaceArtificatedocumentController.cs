﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkplaceArtificatedocumentController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IETMFWorkplaceRepository _eTMFWorkplaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ProjectWorkplaceArtificatedocumentController(IProjectRepository projectRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
              IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
              IUploadSettingRepository uploadSettingRepository,
              IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
               IJwtTokenAccesser jwtTokenAccesser)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _uow = uow;
            _mapper = mapper;
            _eTMFWorkplaceRepository = eTMFWorkplaceRepository;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [Route("GetTreeview")]
        [HttpGet]
        public IActionResult GetTreeview()
        {
            var projectworkplace = _eTMFWorkplaceRepository.GetTreeview(1);
            return Ok(projectworkplace);
        }

        [Route("Get/{id}")]
        [HttpGet]
        public IActionResult Get(int id)
        {
            var reviewDocumentList = _projectWorkplaceArtificateDocumentReviewRepository.GetProjectArtificateDocumentReviewList();
            if (reviewDocumentList == null || reviewDocumentList.Count == 0) return Ok();

            var documentList = _projectWorkplaceArtificatedocumentRepository.FindByInclude(x => x.ProjectWorkplaceArtificateId == id && x.DeletedDate == null && reviewDocumentList.Any(c =>
                                                                                           c == x.Id), x => x.ProjectWorkplaceArtificate).ToList();

            List<CommonArtifactDocumentDto> dataList = new List<CommonArtifactDocumentDto>();
            foreach (var item in documentList)
            {
                var reviewerList = _projectWorkplaceArtificateDocumentReviewRepository.FindByInclude(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id && x.UserId != item.CreatedBy).Select(z => z.UserId).Distinct().ToList();
                var users = new List<string>();
                reviewerList.ForEach(r =>
                {
                    var username = _userRepository.FindByInclude(x => x.Id == r).Select(y => y.UserName);
                    users.AddRange(username);
                });

                CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
                obj.Id = item.Id;
                obj.ProjectWorkplaceSubSectionArtifactId = item.ProjectWorkplaceArtificateId;
                obj.Artificatename = _etmfArtificateMasterLbraryRepository.Find(item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbraryId).ArtificateName;
                obj.ExtendedName = item.DocumentName;
                obj.DocumentName = item.DocumentName.Substring(0, item.DocumentName.LastIndexOf('_'));
                obj.DocPath = System.IO.Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), FolderType.ProjectWorksplace.GetDescription(), item.DocPath, item.DocumentName);
                obj.CreatedByUser = _userRepository.Find((int)item.CreatedBy).UserName;
                obj.Reviewer = string.Join(", ", users);
                obj.CreatedDate = item.CreatedDate;
                obj.Version = item.Version;
                obj.Status = item.Status.GetDescription();
                obj.Level = 6;
                obj.SendBy = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                dataList.Add(obj);
            }
            return Ok(dataList);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectWorkplaceArtificatedocumentDto projectWorkplaceArtificatedocumentDto)
        {
            var Project = _projectRepository.Find(projectWorkplaceArtificatedocumentDto.ProjectId);
            var Projectname = Project.ProjectName + "-" + Project.ProjectCode;

            string filePath = string.Empty;
            string path = string.Empty;

            if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Country.GetDescription(),
                  projectWorkplaceArtificatedocumentDto.Countryname.Trim(), projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());
            else if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Site.GetDescription(),
                 projectWorkplaceArtificatedocumentDto.Sitename.Trim(), projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());
            else if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Trial.GetDescription(),
                   projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());

            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);
            string FileName = DocumentService.SaveWorkplaceDocument(projectWorkplaceArtificatedocumentDto.FileModel, filePath, projectWorkplaceArtificatedocumentDto.FileName);

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            projectWorkplaceArtificatedocumentDto.Id = 0;
            var projectWorkplaceArtificatedocument = _mapper.Map<ProjectWorkplaceArtificatedocument>(projectWorkplaceArtificatedocumentDto);
            projectWorkplaceArtificatedocument.DocumentName = FileName;
            projectWorkplaceArtificatedocument.DocPath = path;
            projectWorkplaceArtificatedocument.Status = ArtifactDocStatusType.Draft;
            projectWorkplaceArtificatedocument.Version = "0.1";

            _projectWorkplaceArtificatedocumentRepository.Add(projectWorkplaceArtificatedocument);
            if (_uow.Save() <= 0) throw new Exception("Creating Document failed on save.");

            _projectWorkplaceArtificateDocumentReviewRepository.SaveByDocumentIdInReview(projectWorkplaceArtificatedocument.Id);
            return Ok(projectWorkplaceArtificatedocument.Id);

        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var subArtifactdoc = _projectWorkplaceArtificatedocumentRepository.FindByInclude(x => x.Id == id).FirstOrDefault();

            if (subArtifactdoc == null)
                return NotFound();
            _projectWorkplaceArtificatedocumentRepository.Delete(subArtifactdoc);
            _uow.Save();
            var aa = _projectWorkplaceArtificatedocumentRepository.deleteFile(id);
            return Ok(aa);
        }
    }
}