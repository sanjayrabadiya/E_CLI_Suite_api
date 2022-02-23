using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class CentralDepotRepository : GenericRespository<CentralDepot>, ICentralDepotRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public CentralDepotRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<DropDownDto> GetStorageAreaByDepoDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.ProjectId == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.StorageArea, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public List<CentralDepotGridDto> GetCentralDepotList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<CentralDepotGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<DropDownDto> GetStorageAreaByProjectDropDown(int ProjectId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.ProjectId == ProjectId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.StorageArea, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(CentralDepot objSave)
        {
            if (objSave.DepotType == DepotType.Central)
            {
                if (All.Any(x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId && x.CountryId == objSave.CountryId && x.StorageArea == objSave.StorageArea.Trim() && x.DeletedDate == null))
                    return "Duplicate Storage Area : " + objSave.StorageArea;
                return "";
            }
            else
            {
                if (All.Any(x => x.Id != objSave.Id && x.SupplyLocationId == objSave.SupplyLocationId && x.StorageArea == objSave.StorageArea.Trim() && x.DeletedDate == null))
                    return "Duplicate Storage Area : " + objSave.StorageArea;
                return "";
            }

        }

        // Study have central depot or not
        public bool IsCentralExists(int ProjectId)
        {
            var exists = All.Any(x => x.ProjectId == ProjectId && x.DepotType == DepotType.Central && x.DeletedDate == null);
            if (exists)
                return true;
            return false;
        }

        // depot use in receipt
        public string ExistsInReceipt(int Id)
        {
            var exists = _context.ProductReceipt.Any(x => x.CentralDepotId == Id && x.DeletedDate == null);
            if (exists)
                return "Receipt is already in use. Cannot delete record!";
            return "";
        }

        // if study use as local area in receipt than can't create central depot
        public string StudyUseInReceipt(CentralDepot objSave)
        {
            var receipt = _context.ProductReceipt.Where(x => x.ProjectId == objSave.ProjectId && x.DeletedDate == null).FirstOrDefault();
            if (receipt != null)
                if (Find(receipt.CentralDepotId).DepotType == DepotType.Local)
                    if (objSave.DepotType == DepotType.Central)
                        return "Can't add central depot for this study, due to already use in reciept as local depot.";
            return "";
        }

        public List<DropDownDto> GetStorageAreaByIdDropDown(int Id)
        {
            var central = Find(Id);
            if (central.ProjectId != null)
                return GetStorageAreaByProjectDropDown((int)central.ProjectId);
            else
                return GetStorageAreaByDepoDropDown();
        }
    }
}
