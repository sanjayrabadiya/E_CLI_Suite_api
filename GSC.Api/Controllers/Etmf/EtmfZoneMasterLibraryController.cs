using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Etmf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class EtmfZoneMasterLibraryController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IEtmfZoneMasterLibraryRepository _etmfZoneMasterLibraryRepository;
        private readonly IEtmfSectionMasterLibraryRepository _etmfSectionMasterLibraryRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLibraryRepository;
        public EtmfZoneMasterLibraryController(
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IEtmfZoneMasterLibraryRepository etmfZoneMasterLibraryRepository,
            IEtmfSectionMasterLibraryRepository etmfSectionMasterLibraryRepository,
            IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLibraryRepository
            )
        {
            _uow = uow;
            _mapper = mapper;
            _etmfZoneMasterLibraryRepository = etmfZoneMasterLibraryRepository;
            _etmfSectionMasterLibraryRepository = etmfSectionMasterLibraryRepository;
            _etmfArtificateMasterLibraryRepository = etmfArtificateMasterLibraryRepository;

        }


        [Route("Get")]
        [HttpGet]
        public ActionResult Get()
        {
            var result = _etmfZoneMasterLibraryRepository.FindByInclude(x => x.DeletedBy == null, x => x.EtmfSectionMasterLibrary);
            return Ok(result);
        }

        [Route("UploadExcel")]
        [HttpPost]
        public ActionResult UploadExcel([FromBody] List<MasterLibraryDto> data)
        {
            List<EtmfZoneMasterLibrary> result = new List<EtmfZoneMasterLibrary>();
            if (data != null)
            {
                result = _etmfZoneMasterLibraryRepository.ExcelDataConvertToEntityformat(data);

                var LastVersiondata = _etmfZoneMasterLibraryRepository.FindByInclude(x => x.DeletedBy == null, x => x.EtmfSectionMasterLibrary).ToList();
                if (LastVersiondata != null && LastVersiondata.Count > 0)
                {
                    foreach (var Lastdata in LastVersiondata)
                    {
                        _etmfZoneMasterLibraryRepository.Delete(Lastdata.Id);
                        foreach (var SectionLast in Lastdata.EtmfSectionMasterLibrary)
                        {
                            _etmfSectionMasterLibraryRepository.Delete(SectionLast);
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
                    foreach (var item in result)
                    {
                        _etmfZoneMasterLibraryRepository.Add(item);
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
            var result = _etmfZoneMasterLibraryRepository.FindBy(x => x.Version == Version);
            return Ok(result);
        }

        [HttpPut]
        public IActionResult Put([FromBody] EtmfZoneMasterLibraryDto etmfZoneMasterLibraryDto)
        {
            if (etmfZoneMasterLibraryDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var etmfZoneMasterLibrary = _mapper.Map<EtmfZoneMasterLibrary>(etmfZoneMasterLibraryDto);
            var validate = _etmfZoneMasterLibraryRepository.Duplicate(etmfZoneMasterLibrary);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _etmfZoneMasterLibraryRepository.Update(etmfZoneMasterLibrary);

            if (_uow.Save() <= 0) throw new Exception("Updating Drug failed on save.");
            return Ok(etmfZoneMasterLibrary.Id);
        }


        [Route("GetOldVersion")]
        [HttpGet]
        public ActionResult GetOldVersion()
        {
            var result = _etmfZoneMasterLibraryRepository.FindBy(x => x.DeletedBy != null).Select(x => x.Version).Distinct();
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
            return Ok(dtolist.OrderByDescending(x=>x.Id));
        }

        [Route("GetInActiveVersionData/{version}")]
        [HttpGet]
        public ActionResult GetInActiveVersionData(string version)
        {
            var result = _etmfZoneMasterLibraryRepository.FindByInclude(x => x.Version == version , x => x.EtmfSectionMasterLibrary);
            return Ok(result);
        }
    }
}