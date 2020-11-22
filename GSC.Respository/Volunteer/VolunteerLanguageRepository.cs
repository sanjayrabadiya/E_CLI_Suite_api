﻿using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Volunteer
{
    public class VolunteerLanguageRepository : GenericRespository<VolunteerLanguage, GscContext>,
        IVolunteerLanguageRepository
    {
        public VolunteerLanguageRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public void RemoveExisting(int id, int volunteerId, int languageId)
        {
            var existingLanguages =
                FindBy(t => t.Id != id && t.VolunteerId == volunteerId && t.LanguageId == languageId).ToList();
            existingLanguages.ForEach(Remove);
        }

        public List<VolunteerLanguageDto> GetLanguages(int volunteerId)
        {
            return FindByInclude(t => t.VolunteerId == volunteerId && t.DeletedDate == null, t => t.Language).Select(
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
    }
}