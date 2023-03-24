using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Respository.Attendance;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class RandomizationNumberSettingsController : ControllerBase
    {
        private readonly IRandomizationNumberSettingsRepository _randomizationNumberSettingsRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;

        public RandomizationNumberSettingsController(IRandomizationNumberSettingsRepository randomizationNumberSettingsRepository,
            IUnitOfWork uow,
            IGSCContext context,
            IMapper mapper)
        {
            _randomizationNumberSettingsRepository = randomizationNumberSettingsRepository;
            _uow = uow;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{projectId}")]
        public IActionResult Get(int projectId)
        {
            if (projectId <= 0) return BadRequest();
            var randomizationNumberSettings = _randomizationNumberSettingsRepository.FindBy(x => x.ProjectId == projectId).FirstOrDefault();
            var randomizationNumberSettingsDto = _mapper.Map<RandomizationNumberSettingsDto>(randomizationNumberSettings);
            var sites = _context.Project.Where(x => x.ParentProjectId == projectId).Select(x => x.Id).ToList();
            var sitesdata = _context.RandomizationNumberSettings.Where(x => sites.Contains(x.ProjectId)).Include(x => x.Project);
            var sitesdataDto = _mapper.Map<List<RandomizationNumberSettingsDto>>(sitesdata);
            sitesdataDto.ForEach(x =>
            {
                var data = _context.Randomization.Where(t => t.ProjectId == x.ProjectId && t.RandomizationNumber != null && t.RandomizationNumber != "").ToList();
                if (data != null && data.Count > 0)
                    x.DisableRow = true;
                else
                    x.DisableRow = false;
            });
            randomizationNumberSettingsDto.RandomizationNumberSettingsSites = sitesdataDto;
            return Ok(randomizationNumberSettingsDto);
        }

        [TransactionRequired]
        [HttpPut("UpdateRandomizationNumberFormat")]
        public IActionResult UpdateRandomizationNumberFormat([FromBody] RandomizationNumberSettingsDto randomizationNumberSettingsDto)
        {
            if (randomizationNumberSettingsDto.Id <= 0) return BadRequest();

            var sites = _context.Project.Where(x => x.ParentProjectId == randomizationNumberSettingsDto.ProjectId && x.IsTestSite == false).Select(y => y.Id).ToList();
            var randomizations = _context.Randomization.Where(x => sites.Contains(x.ProjectId) && x.RandomizationNumber != null && x.RandomizationNumber != "").ToList();

            var randomizationNumberSettings = _randomizationNumberSettingsRepository.Find(randomizationNumberSettingsDto.Id);

            if (randomizations != null && randomizations.Count > 0)
            {
                if (randomizationNumberSettings.RandomNoLength != randomizationNumberSettingsDto.RandomNoLength ||
                    randomizationNumberSettings.IsManualRandomNo != randomizationNumberSettingsDto.IsManualRandomNo ||
                    randomizationNumberSettings.IsSiteDependentRandomNo != randomizationNumberSettingsDto.IsSiteDependentRandomNo)
                {
                    if (randomizationNumberSettingsDto.IsSiteDependentRandomNo == false &&
                        randomizationNumberSettings.RandomNoStartsWith != randomizationNumberSettingsDto.RandomNoStartsWith)
                    {
                        ModelState.AddModelError("Message", "You can't change format, Randomization entry is started in subject management");
                        return BadRequest(ModelState);
                    }
                }
                if (randomizationNumberSettingsDto.IsIGT || randomizationNumberSettingsDto.IsIWRS)
                {
                    ModelState.AddModelError("Message", "You can't change format, Randomization entry is started in subject management");
                    return BadRequest(ModelState);
                }
            }
            if (randomizationNumberSettingsDto.IsIGT == false)
            {
                if (randomizationNumberSettingsDto.IsManualRandomNo == false && randomizationNumberSettingsDto.IsSiteDependentRandomNo == false)
                {
                    if (randomizationNumberSettingsDto.RandomNoStartsWith == null)
                    {
                        ModelState.AddModelError("Message", "Please add valid Starts with number");
                        return BadRequest(ModelState);
                    }
                }
            }


            //var randomizationNumberSettings = _randomizationNumberSettingsRepository.Find(randomizationNumberSettingsDto.Id);
            randomizationNumberSettings.RandomNoLength = randomizationNumberSettingsDto.RandomNoLength;
            randomizationNumberSettings.IsManualRandomNo = randomizationNumberSettingsDto.IsManualRandomNo;
            randomizationNumberSettings.IsAlphaNumRandomNo = randomizationNumberSettingsDto.IsAlphaNumRandomNo;
            randomizationNumberSettings.RandomNoStartsWith = randomizationNumberSettingsDto.RandomNoStartsWith;
            randomizationNumberSettings.IsSiteDependentRandomNo = randomizationNumberSettingsDto.IsSiteDependentRandomNo;
            randomizationNumberSettings.IsIGT = randomizationNumberSettingsDto.IsIGT;
            randomizationNumberSettings.IsIWRS = randomizationNumberSettingsDto.IsIWRS;
            randomizationNumberSettings.PrefixRandomNo = randomizationNumberSettingsDto.PrefixRandomNo;
            if (randomizationNumberSettings.IsManualRandomNo == false && randomizationNumberSettingsDto.IsIGT == false)
            {
                if (randomizationNumberSettings.IsSiteDependentRandomNo == true)
                {
                    for (int i = 0; i < randomizationNumberSettingsDto.RandomizationNumberSettingsSites.Count; i++)
                    {
                        if (randomizationNumberSettingsDto.RandomizationNumberSettingsSites[i].DisableRow == false)
                        {
                            var data = _randomizationNumberSettingsRepository.Find(randomizationNumberSettingsDto.RandomizationNumberSettingsSites[i].Id);
                            data.PrefixRandomNo = randomizationNumberSettingsDto.RandomizationNumberSettingsSites[i].PrefixRandomNo;
                            data.RandomNoStartsWith = randomizationNumberSettingsDto.RandomizationNumberSettingsSites[i].RandomNoStartsWith;
                            data.RandomizationNoseries = (int)data.RandomNoStartsWith;
                            _randomizationNumberSettingsRepository.Update(data);
                        }
                    }
                    //var randomizationNumberSettingssites = _randomizationNumberSettingsRepository.FindBy(x => sites.Contains(x.ProjectId)).ToList();
                    //for (int i = 0; i < randomizationNumberSettingssites.Count; i++)
                    //{
                    //    randomizationNumberSettingssites[i].RandomizationNoseries = (int)randomizationNumberSettings.RandomNoStartsWith;
                    //    _randomizationNumberSettingsRepository.Update(randomizationNumberSettingssites[i]);
                    //}
                }
                else
                {
                    randomizationNumberSettings.RandomizationNoseries = (int)randomizationNumberSettings.RandomNoStartsWith;
                }
            }
            _randomizationNumberSettingsRepository.Update(randomizationNumberSettings);
            if (_uow.Save() <= 0) throw new Exception("Updating Randomization Number failed on save.");
            return Ok(randomizationNumberSettings.Id);
        }

    }
}
