using GSC.Common.GenericRespository;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Entities.AdverseEvent;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.AdverseEvent
{
    public class AdverseEventSettingsLanguageRepository : GenericRespository<AdverseEventSettingsLanguage>, IAdverseEventSettingsLanguageRepository
    {
        public AdverseEventSettingsLanguageRepository(IGSCContext context) : base(context)
        {

        }

        public List<AdverseEventSettingsLanguageSaveDto> GetAdverseEventSettingsLanguage(int AdverseEventSettingsId, int SeqNo)
        {
            var data = All.Where(x => x.AdverseEventSettingsId == AdverseEventSettingsId && x.DeletedDate == null).ToList();
            if (SeqNo == 1)
            {
                data = data.Where(x => x.LowSeverityDisplay != null && x.LowSeverityDisplay != "").ToList();
            }
            else if (SeqNo == 2)
            {
                data = data.Where(x => x.MediumSeverityDisplay != null && x.MediumSeverityDisplay != "").ToList();
            }
            else if (SeqNo == 3)
            {
                data = data.Where(x => x.HighSeverityDisplay != null && x.HighSeverityDisplay != "").ToList();
            }
            List<AdverseEventSettingsLanguageSaveDto> finaldata = new List<AdverseEventSettingsLanguageSaveDto>();
            for (int i = 0; i <= data.Count - 1; i++)
            {
                AdverseEventSettingsLanguageSaveDto obj = new AdverseEventSettingsLanguageSaveDto();
                obj.AdverseEventSettingsLanguageId = data[i].Id;
                obj.LanguageId = data[i].LanguageId;
                if (SeqNo == 1)
                {
                    obj.Display = data[i].LowSeverityDisplay;
                }
                else if (SeqNo == 2)
                {
                    obj.Display = data[i].MediumSeverityDisplay;
                }
                else if (SeqNo == 3)
                {
                    obj.Display = data[i].HighSeverityDisplay;
                }
                finaldata.Add(obj);
            }
            return finaldata;
        }
    }
}
