﻿using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;

namespace GSC.Respository.Volunteer
{
    public class VolunteerLanguageRepository : GenericRespository<VolunteerLanguage>,
        IVolunteerLanguageRepository
    {
        public VolunteerLanguageRepository(IGSCContext context)
            : base(context)
        {
        }

        public void RemoveExisting(int id, int volunteerId, int languageId)
        {
            var existingLanguages =
                FindBy(t => t.Id != id && t.VolunteerId == volunteerId && t.LanguageId == languageId).ToList();
            existingLanguages.ForEach(Remove);
        }

        public List<VolunteerLanguageDto> GetLanguages(int volunteerId, bool isDelete)
        {
            return FindByInclude(t => (isDelete ? t.DeletedDate != null : t.DeletedDate == null) && t.VolunteerId == volunteerId && t.DeletedDate == null, t => t.Language).Select(
                t => new VolunteerLanguageDto
                {
                    Id = t.Id,
                    VolunteerId = t.VolunteerId,
                    LanguageId = t.LanguageId,
                    IsRead = t.IsRead,
                    IsWrite = t.IsWrite,
                    IsSpeak = t.IsSpeak,
                    Note = t.Note,
                    LanguageName = t.Language.LanguageName
                }).OrderByDescending(x => x.Id).ToList();
        }

        public string DuplicateRecord(VolunteerLanguageDto objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.LanguageId == objSave.LanguageId && x.DeletedDate == null && x.VolunteerId == objSave.VolunteerId))
                return "Language Already Exists";
            return "";
        }
    }
}