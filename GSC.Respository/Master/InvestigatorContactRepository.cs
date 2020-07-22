using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class InvestigatorContactRepository : GenericRespository<InvestigatorContact, GscContext>,
        IInvestigatorContactRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public InvestigatorContactRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetInvestigatorContactDropDown(int cityId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.CityId == cityId && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.NameOfInvestigator })
                .OrderBy(o => o.Value).ToList();
        }


        public List<InvestigatorContactDto> GetInvestigatorContact(bool isDeleted)
        {
            var investigatorContact = All.Where(x =>
                (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).Select(c => new InvestigatorContactDto
                {
                    NameOfInvestigator = c.NameOfInvestigator,
                    EmailOfInvestigator = c.EmailOfInvestigator,
                    Specialization = c.Specialization,
                    RegistrationNumber = c.RegistrationNumber,
                    HospitalName = c.HospitalName,
                    HospitalAddress = c.HospitalAddress,
                    ContactNumber = c.ContactNumber,
                    IECIRBName = c.IECIRBName,
                    IECIRBContactNo = c.IECIRBContactNo,
                    IECIRBContactName = c.IECIRBContactName,
                    IECIRBContactEmail = c.IECIRBContactEmail,
                    CityName = c.City.CityName,
                    IsDeleted = c.DeletedDate != null,
                    CountryName = c.City.State.Country.CountryName,
                    StateName = c.City.State.StateName,
                    City = c.City,
                    CityId = c.CityId,
                    Id = c.Id,
                    CountryId = c.City.State.Country.Id,
                    StateId = c.City.State.Id,
                    CompanyId = c.CompanyId
                }).OrderByDescending(x => x.Id).ToList();

            return investigatorContact;
        }

        public string Duplicate(InvestigatorContact objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.RegistrationNumber == objSave.RegistrationNumber && x.DeletedDate == null))
                return "Duplicate RegistrationNumber : " + objSave.RegistrationNumber;
            return "";
        }

        public List<DropDownDto> GetAllInvestigatorContactDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.NameOfInvestigator, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }
    }
}