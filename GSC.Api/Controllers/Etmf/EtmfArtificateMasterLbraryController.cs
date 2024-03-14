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
    public class EtmfArtificateMasterLbraryController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;

        public EtmfArtificateMasterLbraryController(
            IUnitOfWork uow,
            IMapper mapper,
            IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
        }


        [Route("GetArtificateNamebySection/{SectionId}")]
        [HttpGet]
        public ActionResult GetArtificateNamebySection(int SectionId)
        {
            var result = _etmfArtificateMasterLbraryRepository.FindBy(x => x.EtmfSectionMasterLibraryId == SectionId)
                        .OrderBy(y => y.ArtificateNo).ToList();
            return Ok(result);
        }


        [HttpPut]
        public IActionResult Put([FromBody] List<EtmfArtificateMasterLbraryDto> EtmfArtificateMasterLbrary)
        {
            foreach (var item in EtmfArtificateMasterLbrary)
            {
                var etmfArtificateMasterLbrary = _mapper.Map<EtmfArtificateMasterLbrary>(item);
                _etmfArtificateMasterLbraryRepository.AddOrUpdate(etmfArtificateMasterLbrary);
                if (_uow.Save() <= 0)
                {
                    ModelState.AddModelError("Message", "Updating Artificate data failed on save.");
                    return BadRequest(ModelState);
                }
            }
            /* Added by swati for effective Date on 02-06-2019 */
            return Ok(EtmfArtificateMasterLbrary);
        }

        [HttpGet]
        [Route("GetArtificateDropDown/{EtmfSectionMasterLibraryId}/{foldertype}")]
        public IActionResult GetArtificateDropDown(int EtmfSectionMasterLibraryId, short foldertype)
        {
            return Ok(_etmfArtificateMasterLbraryRepository.GetArtificateDropDown(EtmfSectionMasterLibraryId, foldertype));
        }

    }
}