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

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IEtmfZoneMasterLibraryRepository _etmfZoneMasterLibraryRepository;
        private readonly IEtmfSectionMasterLibraryRepository _etmfSectionMasterLibraryRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLibraryRepository;
        public EtmfSectionMasterLibraryController(
            IUnitOfWork uow,
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

        [HttpPut]
        public IActionResult Put([FromBody] EtmfSectionMasterLibraryDto sectionMasterLibraryDto)
        {

            var data = _etmfSectionMasterLibraryRepository.Find(sectionMasterLibraryDto.Id);
            data.SectionName = sectionMasterLibraryDto.SectionName;

            var validate = _etmfSectionMasterLibraryRepository.Duplicate(data);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _etmfSectionMasterLibraryRepository.Update(data);

            if (_uow.Save() <= 0) throw new Exception("Updating Drug failed on save.");
            return Ok(data.Id);
        }
    }
}