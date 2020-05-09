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
    public class MeddraSmqListRepository : GenericRespository<MeddraSmqList, GscContext>, IMeddraSmqListRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MeddraSmqListRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(uow, jwtTokenAccesser)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public int AddSmqListFileData(SaveFileDto obj)
        {
            int count = 0;
            string[] paths = { obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii\\smq_list.asc";
            if (File.Exists(fullPath))
            {
                var Lines = File.ReadAllLines(fullPath);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');
                    Add(new Data.Entities.Medra.MeddraSmqList
                    {
                        MedraConfigId = obj.MedraId,
                        smq_code = Convert.ToInt64(allField[0]),
                        smq_name = allField[1],
                        smq_level = Convert.ToInt32(allField[2]),
                        smq_description = allField[3],
                        smq_source = allField[4],
                        smq_note = allField[5],
                        MedDRA_version = allField[6],
                        status = allField[7],
                        smq_algorithm = allField[8]
                    });
                }
                count = Lines.Length;
            }
            return count;
        }
    }
}
