using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Entities.AdverseEvent;
using GSC.Respository.AdverseEvent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.AdverseEvent
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdverseEventSettingsLanguageController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IAdverseEventSettingsLanguageRepository _adverseEventSettingsLanguageRepository;
        public AdverseEventSettingsLanguageController(IUnitOfWork uow,
            IAdverseEventSettingsLanguageRepository adverseEventSettingsLanguageRepository)
        {
            _uow = uow;
            _adverseEventSettingsLanguageRepository = adverseEventSettingsLanguageRepository;
        }

        [HttpGet]
        [Route("GetAdverseEventSettingsLanguage/{AdverseEventSettingsId}/{SeqNo}")]
        public IActionResult GetAdverseEventSettingsLanguage(int AdverseEventSettingsId,int SeqNo)
        {
            var templateLanguage = _adverseEventSettingsLanguageRepository.GetAdverseEventSettingsLanguage(AdverseEventSettingsId,SeqNo);
            return Ok(templateLanguage);
        }

        [HttpPost]
        public IActionResult Post([FromBody] AdverseEventSettingsLanguageDto adverseEventSettingsLanguageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in adverseEventSettingsLanguageDto.adverseEventSettingsLanguages)
            {
                AdverseEventSettingsLanguage obj = new AdverseEventSettingsLanguage();
                obj.Id = item.AdverseEventSettingsLanguageId;
                var data = _adverseEventSettingsLanguageRepository.All.Where(x => x.AdverseEventSettingsId == adverseEventSettingsLanguageDto.AdverseEventSettingsId &&
                                            x.LanguageId == item.LanguageId).ToList();
                if (data != null && data.Count > 0)
                {
                    obj.Id = data.FirstOrDefault().Id;
                    obj = _adverseEventSettingsLanguageRepository.Find(obj.Id);
                }
                obj.AdverseEventSettingsId = adverseEventSettingsLanguageDto.AdverseEventSettingsId;
                obj.LanguageId = item.LanguageId;
                if (item.SeqNo == 1)
                {
                    obj.LowSeverityDisplay = item.Display;
                } else if (item.SeqNo == 2)
                {
                    obj.MediumSeverityDisplay = item.Display;
                } else if (item.SeqNo == 3)
                {
                    obj.HighSeverityDisplay = item.Display;
                }
                if (data != null && data.Count > 0)
                {
                    obj.Id = data.FirstOrDefault().Id;
                    _adverseEventSettingsLanguageRepository.Update(obj);
                } else
                {
                    _adverseEventSettingsLanguageRepository.Add(obj);
                }
            }
            if (_uow.Save() <= 0) throw new Exception("Creating Adverse Event language failed on save.");
            return Ok();
        }

        [HttpPost]
        [Route("RemoveLanguage/{AdverseEventSettingsLanguageId}/{SeqNo}")]
        public IActionResult RemoveLanguage(int AdverseEventSettingsLanguageId, int SeqNo)
        {
            var data = _adverseEventSettingsLanguageRepository.Find(AdverseEventSettingsLanguageId);
            if (SeqNo == 1)
                data.LowSeverityDisplay = null;
            else if (SeqNo == 2)
                data.MediumSeverityDisplay = null;
            else if (SeqNo == 3)
                data.HighSeverityDisplay = null;
            _adverseEventSettingsLanguageRepository.Update(data);
            if (_uow.Save() <= 0) throw new Exception("Error occured on remove language");
            return Ok();
        }
    }
}
