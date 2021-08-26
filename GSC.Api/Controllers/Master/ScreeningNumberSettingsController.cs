﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScreeningNumberSettingsController : ControllerBase
    {
        private readonly IScreeningNumberSettingsRepository _screeningNumberSettingsRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        public ScreeningNumberSettingsController(IScreeningNumberSettingsRepository screeningNumberSettingsRepository,
            IUnitOfWork uow,
            IGSCContext context,
            IMapper mapper)
        {
            _screeningNumberSettingsRepository = screeningNumberSettingsRepository;
            _uow = uow;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{projectId}")]
        public IActionResult Get(int projectId)
        {
            if (projectId <= 0) return BadRequest();
            var screeningNumberSettings = _screeningNumberSettingsRepository.FindBy(x => x.ProjectId == projectId).FirstOrDefault();
            var screeningNumberSettingsDto = _mapper.Map<ScreeningNumberSettingsDto>(screeningNumberSettings);
            var sites = _context.Project.Where(x => x.ParentProjectId == projectId).Select(x => x.Id).ToList();
            var sitesdata = _context.ScreeningNumberSettings.Where(x => sites.Contains(x.ProjectId)).Include(x => x.Project);
            var sitesdataDto = _mapper.Map<List<ScreeningNumberSettingsDto>>(sitesdata);
            sitesdataDto.ForEach(x =>
            {
                var data = _context.Randomization.Where(t => t.ProjectId == x.ProjectId && t.ScreeningNumber != null && t.ScreeningNumber != "").ToList();
                if (data != null && data.Count > 0)
                    x.DisableRow = true;
                else
                    x.DisableRow = false;
            });
            screeningNumberSettingsDto.ScreeningNumberSettingsSites = sitesdataDto;
            return Ok(screeningNumberSettingsDto);
        }

        [TransactionRequired]
        [HttpPut("UpdateScreeningNumberFormat")]
        public IActionResult UpdateScreeningNumberFormat([FromBody] ScreeningNumberSettingsDto screeningNumberSettingsDto)
        {
            if (screeningNumberSettingsDto.Id <= 0) return BadRequest();

            var sites = _context.Project.Where(x => x.ParentProjectId == screeningNumberSettingsDto.ProjectId && x.IsTestSite == false).Select(y => y.Id).ToList();
            var randomizations = _context.Randomization.Where(x => sites.Contains(x.ProjectId) && x.ScreeningNumber != null && x.ScreeningNumber != "").ToList();
            var screeningNumberSettings = _screeningNumberSettingsRepository.Find(screeningNumberSettingsDto.Id);
            if (randomizations != null && randomizations.Count > 0)
            {
                if (screeningNumberSettings.ScreeningLength != screeningNumberSettingsDto.ScreeningLength ||
                    screeningNumberSettings.IsManualScreeningNo != screeningNumberSettingsDto.IsManualScreeningNo ||
                    screeningNumberSettings.IsSiteDependentScreeningNo != screeningNumberSettingsDto.IsSiteDependentScreeningNo)
                {
                    if (screeningNumberSettingsDto.IsSiteDependentScreeningNo == false &&
                        screeningNumberSettings.ScreeningNoStartsWith != screeningNumberSettingsDto.ScreeningNoStartsWith)
                    {
                        ModelState.AddModelError("Message", "You can't change format, Screening entry is started in subject management");
                        return BadRequest(ModelState);
                    }
                }  
            }

            if (screeningNumberSettingsDto.IsManualScreeningNo == false && screeningNumberSettingsDto.IsSiteDependentScreeningNo == false)
            {
                if (screeningNumberSettingsDto.ScreeningNoStartsWith == null)
                {
                    ModelState.AddModelError("Message", "Please add valid Starts with number");
                    return BadRequest(ModelState);
                }
            }

            //var screeningNumberSettings = _screeningNumberSettingsRepository.Find(screeningNumberSettingsDto.Id);
            screeningNumberSettings.ScreeningLength = screeningNumberSettingsDto.ScreeningLength;
            screeningNumberSettings.IsManualScreeningNo = screeningNumberSettingsDto.IsManualScreeningNo;
            screeningNumberSettings.IsAlphaNumScreeningNo = screeningNumberSettingsDto.IsAlphaNumScreeningNo;
            screeningNumberSettings.ScreeningNoStartsWith = screeningNumberSettingsDto.ScreeningNoStartsWith;
            screeningNumberSettings.IsSiteDependentScreeningNo = screeningNumberSettingsDto.IsSiteDependentScreeningNo;

            if (screeningNumberSettings.IsManualScreeningNo == false)
            {
                if (screeningNumberSettings.IsSiteDependentScreeningNo == true)
                {
                    for (int i = 0; i < screeningNumberSettingsDto.ScreeningNumberSettingsSites.Count; i++)
                    {
                        if (screeningNumberSettingsDto.ScreeningNumberSettingsSites[i].DisableRow == false)
                        {
                            var data = _screeningNumberSettingsRepository.Find(screeningNumberSettingsDto.ScreeningNumberSettingsSites[i].Id);
                            data.PrefixScreeningNo = screeningNumberSettingsDto.ScreeningNumberSettingsSites[i].PrefixScreeningNo;
                            data.ScreeningNoStartsWith = screeningNumberSettingsDto.ScreeningNumberSettingsSites[i].ScreeningNoStartsWith;
                            data.ScreeningNoseries = (int)data.ScreeningNoStartsWith;
                            _screeningNumberSettingsRepository.Update(data);
                        }
                    }
                    //var screeningNumberSettingssites = _screeningNumberSettingsRepository.FindBy(x => sites.Contains(x.ProjectId)).ToList();
                    //for (int i = 0; i < screeningNumberSettingssites.Count; i++)
                    //{
                    //    screeningNumberSettingssites[i].ScreeningNoseries = (int)screeningNumberSettings.ScreeningNoStartsWith;
                    //    _screeningNumberSettingsRepository.Update(screeningNumberSettingssites[i]);
                    //}
                }
                else
                {
                    screeningNumberSettings.ScreeningNoseries = (int)screeningNumberSettings.ScreeningNoStartsWith;
                }
            }
            _screeningNumberSettingsRepository.Update(screeningNumberSettings);
            if (_uow.Save() <= 0) throw new Exception("Updating Project failed on save.");
            return Ok(screeningNumberSettings.Id);
        }
    }
}
