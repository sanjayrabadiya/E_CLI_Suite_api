using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Respository.Etmf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    [ApiController]
    public class EtmfMasterLibraryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IEtmfMasterLbraryRepository _etmfMasterLibraryRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLibraryRepository;
        public EtmfMasterLibraryController(
            IUnitOfWork uow,
            IMapper mapper,
            IEtmfMasterLbraryRepository etmfMasterLibraryRepository,
            IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLibraryRepository
            )
        {
            _uow = uow;
            _mapper = mapper;
            _etmfMasterLibraryRepository = etmfMasterLibraryRepository;
            _etmfArtificateMasterLibraryRepository = etmfArtificateMasterLibraryRepository;

        }

        [HttpPut]
        public IActionResult Put([FromBody] EtmfMasterLibraryDto masterLibraryDto)
        {

            var data = _etmfMasterLibraryRepository.Find(masterLibraryDto.Id);
            data.ZonName = masterLibraryDto.ZonName;
            data.SectionName = masterLibraryDto.SectionName;

            var validate = _etmfMasterLibraryRepository.Duplicate(data);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _etmfMasterLibraryRepository.Update(data);

            if (_uow.Save() <= 0) throw new Exception("Updating Drug failed on save.");
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
