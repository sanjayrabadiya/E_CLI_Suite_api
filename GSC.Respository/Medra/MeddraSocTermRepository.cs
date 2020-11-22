using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Respository.PropertyMapping;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MeddraSocTermRepository : GenericRespository<MeddraSocTerm, GscContext>, IMeddraSocTermRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MeddraSocTermRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(uow, jwtTokenAccesser)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public int AddSocFileData(SaveFileDto obj)
        {
            int count = 0;
            string[] paths = { obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii\\soc.asc";
            if (File.Exists(fullPath))
            {
                var Lines = File.ReadAllLines(fullPath);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');
                    Add(new Data.Entities.Medra.MeddraSocTerm
                    {
                        MedraConfigId = obj.MedraId,
                        soc_code = Convert.ToInt64(allField[0]),
                        soc_name = allField[1],
                        soc_abbrev = allField[2],
                        soc_harts_code = allField[3] == "" ? (long?)null : Convert.ToInt64(allField[3]),
                        soc_costart_sym = allField[4],
                        soc_icd9_code = allField[5],
                        soc_icd9cm_code = allField[6],
                        soc_icd10_code = allField[7],
                        soc_jart_code = allField[8]
                    });
                }
                count = Lines.Length;
            }
            return count;
        }
    }
}
