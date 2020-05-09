using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.PropertyMapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MeddraPrefTermRepository : GenericRespository<MeddraPrefTerm, GscContext>, IMeddraPrefTermRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MeddraPrefTermRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(uow, jwtTokenAccesser)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public int AddPtFileData(SaveFileDto obj)
        {
            int count = 0;
            string[] paths = { obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii\\pt.asc";
            if (File.Exists(fullPath))
            {
                var Lines = File.ReadAllLines(fullPath);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');
                    Add(new Data.Entities.Medra.MeddraPrefTerm
                    {
                        MedraConfigId = obj.MedraId,
                        pt_code = Convert.ToInt64(allField[0]),
                        pt_name = allField[1],
                        null_field = allField[2],
                        pt_soc_code = allField[3] == "" ? (long?)null : Convert.ToInt64(allField[3]),
                        pt_whoart_code = allField[4],
                        pt_harts_code = allField[5] == "" ? (long?)null : Convert.ToInt64(allField[5]),
                        pt_costart_sym = allField[6],
                        pt_icd9_code = allField[7],
                        pt_icd9cm_code = allField[8],
                        pt_icd10_code = allField[9],
                        pt_jart_code = allField[10]
                    });
                }
                count = Lines.Length;
            }
            return count;
        }
    }
}
