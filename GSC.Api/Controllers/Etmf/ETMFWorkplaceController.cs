using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.Generic;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IUnitOfWork _uow;
        private readonly IETMFWorkplaceRepository _eTMFWorkplaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectWorkplaceDetailRepository _projectWorkplaceDetailRepository;
        private readonly IProjectWorkPlaceZoneRepository _projectWorkPlaceZoneRepository;
        private readonly IProjectWorkplaceSectionRepository _projectWorkplaceSectionRepository;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;


        private readonly IEtmfUserPermissionRepository _etmfUserPermissionRepository;
        public ETMFWorkplaceController(IProjectRepository projectRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            ICountryRepository countryRepository,
            IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProjectWorkplaceDetailRepository projectWorkplaceDetailRepository,
            IProjectWorkPlaceZoneRepository projectWorkPlaceZoneRepository,
            IProjectWorkplaceSectionRepository projectWorkplaceSectionRepository,
            IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
            IEtmfUserPermissionRepository etmfUserPermissionRepository
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
            _projectWorkplaceDetailRepository = projectWorkplaceDetailRepository;
            _projectWorkPlaceZoneRepository = projectWorkPlaceZoneRepository;
            _projectWorkplaceSectionRepository = projectWorkplaceSectionRepository;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
            _etmfUserPermissionRepository = etmfUserPermissionRepository;

        }

        [Route("Get")]
        [HttpGet]
        public IActionResult Get(bool isDeleted)
        {
            var projects = _eTMFWorkplaceRepository.GetETMFWorkplaceList(isDeleted);
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var EtmfProjectWorkPlace = _eTMFWorkplaceRepository.All.Include(x => x.Project);
            return Ok(EtmfProjectWorkPlace);
        }

        [Route("GetTreeview/{projectId}/{chartType:int?}")]
        [HttpGet]
        public IActionResult GetTreeview(int projectId, EtmfChartType? chartType)
        {
            var EtmfProjectWorkPlace = _eTMFWorkplaceRepository.GetTreeview(projectId, chartType);
            return Ok(EtmfProjectWorkPlace);
        }
        [Route("GetEtmfSearchData/{id}")]
        [HttpGet]
        public IActionResult GetEtmfSearchData(int id)
        {
            var EtmfProjectWorkPlace = _eTMFWorkplaceRepository.GetEtmfSearchData(id);
            return Ok(EtmfProjectWorkPlace);
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
            SaveFolderStructure.Version = artificiteList.FirstOrDefault().Version;
            SaveFolderStructure.TableTag = (int)EtmfTableNameTag.ProjectWorkPlace;
            _eTMFWorkplaceRepository.Add(SaveFolderStructure);
            foreach (var workplaceDetail in SaveFolderStructure.ProjectWorkplaceDetails)
            {
                workplaceDetail.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceDetail;
                workplaceDetail.Version = SaveFolderStructure.Version;
                workplaceDetail.ProjectId = SaveFolderStructure.ProjectId;
                _projectWorkplaceDetailRepository.Add(workplaceDetail);

                foreach (var zone in workplaceDetail.ProjectWorkplaceDetails)
                {
                    zone.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceZone;
                    zone.Version = SaveFolderStructure.Version;
                    zone.ProjectId = SaveFolderStructure.ProjectId;
                    _projectWorkPlaceZoneRepository.Add(zone);

                    foreach (var section in zone.ProjectWorkplaceDetails)
                    {
                        section.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceSection;
                        section.Version = SaveFolderStructure.Version;
                        section.ProjectId = SaveFolderStructure.ProjectId;
                        _projectWorkplaceSectionRepository.Add(section);

                        foreach (var artificate in section.ProjectWorkplaceDetails)
                        {
                            artificate.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceArtificate;
                            artificate.Version = SaveFolderStructure.Version;
                            artificate.ProjectId = SaveFolderStructure.ProjectId;
                            _projectWorkplaceArtificateRepository.Add(artificate);
                        }
                    }
                }
            }
            if (_uow.Save() <= 0) throw new Exception("Creating ETMFWorkplace failed on save.");
            _etmfUserPermissionRepository.AddEtmfAccessRights(SaveFolderStructure.ProjectWorkplaceDetails.ToList());
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

        [HttpGet]
        [Route("DownloadFolder/{Id}")]
        public IActionResult DownloadFolder(int Id)
        {
            var filebytes = _eTMFWorkplaceRepository.CreateZipFileOfWorkplace(Id);
            return File(filebytes, "application/zip");
        }

        [HttpGet]
        [Route("DownloadFolderFromJobMonitoring/{Id}")]
        public IActionResult DownloadFolderFromJobMonitoring(int Id)
        {
            _eTMFWorkplaceRepository.CreateZipFileOfWorkplaceJobMonitoring(Id);
            return Ok();
        }

        [HttpPost]
        [Route("SaveSiteData")]
        public IActionResult SaveSiteData([FromBody] List<int> ProjectIds)
        {
            var ParentProjectId = _projectRepository.FindByInclude(x => x.Id == ProjectIds[0]).FirstOrDefault().ParentProjectId;
            var projectDetail = _projectRepository.Find((int)ParentProjectId);
            var childProjectList = ProjectIds;
            var countryList = _countryRepository.GetCountryByProjectIdDropDown((int)ParentProjectId);
            var artificiteList = _etmfArtificateMasterLbraryRepository.GetArtifcateWithAllListByVersion((int)ParentProjectId);
            var imageUrl = _uploadSettingRepository.GetDocumentPath();

            var SaveFolderStructure = _eTMFWorkplaceRepository.SaveSiteFolderStructure(projectDetail, childProjectList, countryList, artificiteList, imageUrl);
            SaveFolderStructure.Version = artificiteList.FirstOrDefault().Version;
            SaveFolderStructure.TableTag = (int)EtmfTableNameTag.ProjectWorkPlace;
            foreach (var workplaceDetail in SaveFolderStructure.ProjectWorkplaceDetails)
            {
                workplaceDetail.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceDetail;
                workplaceDetail.Version = SaveFolderStructure.Version;
                workplaceDetail.ProjectId = SaveFolderStructure.ProjectId;
                _projectWorkplaceDetailRepository.Add(workplaceDetail);

                foreach (var zone in workplaceDetail.ProjectWorkplaceDetails)
                {
                    zone.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceZone;
                    zone.Version = SaveFolderStructure.Version;
                    zone.ProjectId = SaveFolderStructure.ProjectId;
                    _projectWorkPlaceZoneRepository.Add(zone);

                    foreach (var section in zone.ProjectWorkplaceDetails)
                    {
                        section.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceSection;
                        section.Version = SaveFolderStructure.Version;
                        section.ProjectId = SaveFolderStructure.ProjectId;
                        _projectWorkplaceSectionRepository.Add(section);

                        foreach (var artificate in section.ProjectWorkplaceDetails)
                        {
                            artificate.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceArtificate;
                            artificate.Version = SaveFolderStructure.Version;
                            artificate.ProjectId = SaveFolderStructure.ProjectId;
                            _projectWorkplaceArtificateRepository.Add(artificate);
                        }
                    }
                }
            }

            if (_uow.Save() <= 0) throw new Exception("Creating ETMFWorkplace failed on save.");
            return Ok(SaveFolderStructure.Id);
        }

        [Route("GetChartReport/{projectId}/{chartType:int?}")]
        [HttpGet]
        public IActionResult GetChartReport(int projectId, EtmfChartType? chartType)
        {
            var EtmfProjectWorkPlace = _eTMFWorkplaceRepository.GetChartReport(projectId, chartType);
            return Ok(EtmfProjectWorkPlace);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            //var record = _eTMFWorkplaceRepository.GetWorkplaceDetails(id);
            //if (record == null)
            //    return NotFound();

            //_eTMFWorkplaceRepository.Delete(record.Id);

            //_eTMFWorkplaceRepository.DeleteAllTable(record);
            _eTMFWorkplaceRepository.DeleteAllEtmfTableRecords(id);
            _uow.Save();
            return Ok();
        }
        [Route("DownloadPdfFile/{path}")]
        [AllowAnonymous]
        [HttpGet]
        public FileResult DownloadPdfFile(string path)
        {
            var actualPath = path.Replace("^", "\\");
            var result = _eTMFWorkplaceRepository.DownloadPdf(actualPath);
            var fileName = Path.GetFileName(actualPath);
            return File(result, "application/octet-stream", fileName);
        }
    }
}