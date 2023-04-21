using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Barcode;
using System.Collections.Generic;

namespace GSC.Respository.Barcode
{
    public interface ISampleSeparationRepository : IGenericRepository<SampleSeparation>
    {
        List<SampleSeparationGridDto> GetSampleDetails(int siteId, int templateId);
        void StartSampleSaparation(SampleSaveSeparationDto dto);
        IList<DropDownDto> GetTemplateForSaparation(int siteId);
    }
}