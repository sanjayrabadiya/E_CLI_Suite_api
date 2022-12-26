using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementFectorDetailRepository : GenericRespository<SupplyManagementFectorDetail>, ISupplyManagementFectorDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementFectorDetailRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }
        public SupplyManagementFectorDto GetDetailList(int id)
        {
            var data = _context.SupplyManagementFector.Include(x => x.FectorDetailList).Where(x => x.Id == id).Select(x => new SupplyManagementFectorDto
            {
                Id = x.Id,
                SourceFormula = x.SourceFormula,
                CheckFormula = x.CheckFormula,
                ErrorMessage = x.ErrorMessage,
                SampleResult = x.SampleResult,
                Children = x.FectorDetailList.Where(x => x.DeletedDate == null).Select(z => new SupplyManagementFectorDetailDto
                {
                    Id = z.Id,
                    SupplyManagementFectorId = z.SupplyManagementFectorId,
                    ProductTypeCode = z.ProductTypeCode,
                    Fector = z.Fector,
                    Operator = z.Operator,
                    CollectionValue = z.CollectionValue,
                    LogicalOperator = z.LogicalOperator,
                    Ratio = z.Ratio,
                    FactoreName = z.Fector.GetDescription(),
                    FactoreOperatorName = z.Operator.ToString(),
                    collectionValueName = GetCollectionValue(z.Fector, z.CollectionValue),
                    Type = z.Type,
                    TypeName = z.Type.GetDescription(),
                    IsDeleted = z.DeletedDate != null ? true : false,
                    ProjectCode = z.SupplyManagementFector.Project.ProjectCode
                }).ToList()
            }).FirstOrDefault();
            return data;
        }
        public SupplyManagementFectorDetailDto GetDetail(int id)
        {

            var data = All.Where(x => x.Id == id).Select(z => new SupplyManagementFectorDetailDto
            {
                Id = z.Id,
                SupplyManagementFectorId = z.SupplyManagementFectorId,
                ProductTypeCode = z.ProductTypeCode,
                Fector = z.Fector,
                Operator = z.Operator,
                CollectionValue = z.CollectionValue,
                LogicalOperator = z.LogicalOperator,
                Ratio = z.Ratio,
                FactoreName = z.Fector.GetDescription(),
                FactoreOperatorName = z.Operator.ToString(),
                collectionValueName = GetCollectionValue(z.Fector, z.CollectionValue),
                Type = z.Type,
                TypeName = z.Type.GetDescription(),
            }).FirstOrDefault();
            return data;
        }

        public static string GetCollectionValue(Fector fector, string Collectionavalue)
        {
            if (fector == Fector.Gender)
            {
                if (Collectionavalue != null)
                {
                    if (Collectionavalue == "1")
                    {
                        return "Male";
                    }
                    if (Collectionavalue == "2")
                    {
                        return "Female";
                    }
                }
            }
            if (fector == Fector.Diatory)
            {
                if (Collectionavalue != null)
                {
                    if (Collectionavalue == "1")
                    {
                        return "Veg";
                    }
                    if (Collectionavalue == "2")
                    {
                        return "Non-veg";
                    }
                }
            }
            if (fector == Fector.BMI || fector == Fector.Age)
            {
                return Collectionavalue;
            }
            return "";

        }

        public bool CheckType(SupplyManagementFectorDetailDto supplyManagementFectorDetailDto)
        {
            var data = _context.SupplyManagementFectorDetail.Where(x => x.SupplyManagementFectorId == supplyManagementFectorDetailDto.SupplyManagementFectorId).FirstOrDefault();
            if (data != null && data.Type != supplyManagementFectorDetailDto.Type)
            {
                return false;
            }
            return true;
        }
    }
}
