using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Etmf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class EtmfSectionMasterLibraryController : BaseController
    {

        private readonly IUnitOfWork _uow;
        private readonly IEtmfMasterLbraryRepository _etmfMasterLibraryRepository;
        public EtmfSectionMasterLibraryController(
            IUnitOfWork uow,
            IEtmfMasterLbraryRepository etmfMasterLibraryRepository
            )
        {
            _uow = uow;
            _etmfMasterLibraryRepository = etmfMasterLibraryRepository;
        }

        [HttpPut]
        public IActionResult Put([FromBody] EtmfMasterLibraryDto sectionMasterLibraryDto)
        {

            var data = _etmfMasterLibraryRepository.Find(sectionMasterLibraryDto.Id);
            data.SectionName = sectionMasterLibraryDto.SectionName;

            var validate = _etmfMasterLibraryRepository.Duplicate(data);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _etmfMasterLibraryRepository.Update(data);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Drug failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(data.Id);
        }


        [HttpGet]
        [Route("GetSectionMasterLibraryDropDown/{EtmfZoneMasterLibraryId}")]
        public IActionResult GetSectionMasterLibraryDropDown(int EtmfZoneMasterLibraryId)
        {
            return Ok(_etmfMasterLibraryRepository.GetSectionMasterLibraryDropDown(EtmfZoneMasterLibraryId));
        }
    }
}