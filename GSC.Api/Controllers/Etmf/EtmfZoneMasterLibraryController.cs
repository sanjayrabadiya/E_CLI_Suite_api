﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class EtmfZoneMasterLibraryController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IEtmfMasterLbraryRepository _etmfMasterLibraryRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLibraryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public EtmfZoneMasterLibraryController(
            IUnitOfWork uow,
            IEtmfMasterLbraryRepository etmfMasterLibraryRepository,
            IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLibraryRepository,
                IUploadSettingRepository uploadSettingRepository
            )
        {
            _uow = uow;
            _etmfMasterLibraryRepository = etmfMasterLibraryRepository;
            _etmfArtificateMasterLibraryRepository = etmfArtificateMasterLibraryRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }


        [Route("Get")]
        [HttpGet]
        public ActionResult Get()
        {
            var result = _etmfMasterLibraryRepository.FindByInclude(x => x.DeletedBy == null && x.EtmfMasterLibraryId == 0, x => x.EtmfSectionMasterLibrary).OrderBy(x =>
            {
                x.EtmfSectionMasterLibrary = x.EtmfSectionMasterLibrary.OrderBy(y => y.Sectionno).ToList();
                return x.Id;
            });

            return Ok(result);
        }

        [Route("UploadExcel")]
        [HttpPost]
        public ActionResult UploadExcel([FromBody] List<MasterLibraryDto> data)
        {
            List<EtmfMasterLibrary> result = new List<EtmfMasterLibrary>();
            if (data != null)
            {
                result = _etmfMasterLibraryRepository.ExcelDataConvertToEntityformat(data);

                var LastVersiondata = _etmfMasterLibraryRepository.FindByInclude(x => x.DeletedBy == null, x => x.EtmfSectionMasterLibrary).ToList();
                if (LastVersiondata.Any())
                {
                    foreach (var Lastdata in LastVersiondata)
                    {
                        _etmfMasterLibraryRepository.Delete(Lastdata);
                        foreach (var SectionLast in Lastdata.EtmfSectionMasterLibrary)
                        {
                            _etmfMasterLibraryRepository.Delete(SectionLast);
                            var LastArtificateVersiondata = _etmfArtificateMasterLibraryRepository.FindBy(x => x.EtmfSectionMasterLibraryId == SectionLast.Id).ToList();
                            foreach (var ArtificateLast in LastArtificateVersiondata)
                            {
                                _etmfArtificateMasterLibraryRepository.Delete(ArtificateLast);
                            }
                        }
                        _uow.Save();
                    }
                }

                if (result != null)
                {
                    string filePath = string.Empty;
                    filePath = System.IO.Path.Combine(_uploadSettingRepository.GetImagePath(), "DossierReport");
                    string FileName = DocumentService.SaveETMFDocument(data[0].fileModel, filePath, Helper.FolderType.ExcleTemplate, result[0].Version);
                    foreach (var item in result)
                    {
                        item.FileName = FileName;
                        item.EtmfMasterLibraryId = 0;
                        _etmfMasterLibraryRepository.Add(item);

                        foreach (var section in item.EtmfSectionMasterLibrary)
                        {
                            section.EtmfMasterLibraryId = item.Id;
                            _etmfMasterLibraryRepository.Add(section);

                            foreach (var Artificate in section.EtmfArtificateMasterLbrary)
                            {
                                _etmfArtificateMasterLibraryRepository.Add(Artificate);
                            }
                        }
                        _uow.Save();
                    }
                }
            }
            return Ok(result);
        }


        [Route("CheckETMFVersion/{Version}")]
        [HttpGet]
        public ActionResult CheckETMFVersion(string Version)
        {
            var result = _etmfMasterLibraryRepository.FindBy(x => x.Version == Version);
            return Ok(result);
        }

        [HttpPut]
        public IActionResult Put([FromBody] EtmfMasterLibraryDto etmfZoneMasterLibraryDto)
        {

            var data = _etmfMasterLibraryRepository.Find(etmfZoneMasterLibraryDto.Id);
            data.ZonName = etmfZoneMasterLibraryDto.ZonName;
            var validate = _etmfMasterLibraryRepository.Duplicate(data);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _etmfMasterLibraryRepository.Update(data);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Drug failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(data.Id);
        }


        [Route("GetOldVersion")]
        [HttpGet]
        public ActionResult GetOldVersion()
        {
            var result = _etmfMasterLibraryRepository.FindBy(x => x.DeletedBy != null && x.EtmfMasterLibraryId == 0).Select(x => x.Version).Distinct();
            int cnt = 1;
            List<DropDownDto> dtolist = new List<DropDownDto>();
            foreach (var val in result)
            {
                DropDownDto obj = new DropDownDto();
                obj.Id = cnt;
                obj.Value = val.ToString();
                cnt++;
                dtolist.Add(obj);
            }
            return Ok(dtolist.OrderByDescending(x => x.Id));
        }

        [Route("GetVersion")]
        [HttpGet]
        public ActionResult GetVersion()
        {
            var result = _etmfMasterLibraryRepository.All.Select(x => x.Version).Distinct();
            int cnt = 1;
            List<DropDownDto> dtolist = new List<DropDownDto>();
            foreach (var val in result)
            {
                DropDownDto obj = new DropDownDto();
                obj.Id = cnt;
                obj.Value = val.ToString();
                cnt++;
                dtolist.Add(obj);
            }
            return Ok(dtolist.OrderByDescending(x => x.Id));
        }

        [Route("GetInActiveVersionData/{version}")]
        [HttpGet]
        public ActionResult GetInActiveVersionData(string version)
        {
            var result = _etmfMasterLibraryRepository.FindByInclude(x => x.Version == version, x => x.EtmfSectionMasterLibrary);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetZoneMasterLibraryDropDown/{version}")]
        public IActionResult GetZoneMasterLibraryDropDown(string version)
        {
            return Ok(_etmfMasterLibraryRepository.GetZoneMasterLibraryDropDown(version));
        }
    }
}