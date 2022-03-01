﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyMangementShipmentController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementShipmentRepository _supplyManagementShipmentRepository;
        private readonly IUnitOfWork _uow;

        public SupplyMangementShipmentController(ISupplyManagementShipmentRepository supplyManagementShipmentRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _supplyManagementShipmentRepository = supplyManagementShipmentRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementShipmentDto supplyManagementshipmentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            supplyManagementshipmentDto.Id = 0;
            var supplyManagementRequest = _mapper.Map<SupplyManagementShipment>(supplyManagementshipmentDto);
            bool isnotexist = false;
            while (!isnotexist)
            {
                var str = _supplyManagementShipmentRepository.GenerateShipmentNo();
                if (!string.IsNullOrEmpty(str))
                {
                    var data = _supplyManagementShipmentRepository.All.Where(x => x.ShipmentNo == str).FirstOrDefault();
                    if (data == null)
                    {
                        isnotexist = true;
                        supplyManagementRequest.ShipmentNo = str;
                        break;
                    }
                }

            }
            _supplyManagementShipmentRepository.Add(supplyManagementRequest);
            if (_uow.Save() <= 0) throw new Exception("Creating shipment failed on save.");
            return Ok(supplyManagementRequest.Id);
        }
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var data = _supplyManagementShipmentRepository.GetSupplyShipmentList(isDeleted);
            return Ok(data);
        }
    }
}
