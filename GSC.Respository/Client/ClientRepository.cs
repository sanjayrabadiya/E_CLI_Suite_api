﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Client;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Client
{
    public class ClientRepository : GenericRespository<Data.Entities.Client.Client>, IClientRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IMapper _mapper;

        public ClientRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IUploadSettingRepository uploadSettingRepository,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
            _mapper = mapper;
        }

        public List<DropDownDto> GetClientDropDown()
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.ClientName, ExtraData= c.Logo ==null?documentUrl+ "2\\Client\\Logo\\Original\\uploadClientLogo.png" : documentUrl + c.Logo, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string DuplicateClient(Data.Entities.Client.Client objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ClientCode == objSave.ClientCode.Trim() && x.DeletedDate == null))
                return "Duplicate Client code : " + objSave.ClientCode;

            return "";
        }

        public List<ClientGridDto> GetClientList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<ClientGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}