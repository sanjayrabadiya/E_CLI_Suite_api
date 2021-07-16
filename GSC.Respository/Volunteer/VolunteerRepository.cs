using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Screening;
using GSC.Data.Dto.Volunteer;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using Microsoft.EntityFrameworkCore;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace GSC.Respository.Volunteer
{
    public class VolunteerRepository : GenericRespository<Data.Entities.Volunteer.Volunteer>,
        IVolunteerRepository
    {
        private readonly ICityRepository _cityRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectDesignPeriodRepository _projectDesignPeriodRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;

        public VolunteerRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            INumberFormatRepository numberFormatRepository,
            IUploadSettingRepository uploadSettingRepository,
            ICityRepository cityRepository,
            ICompanyRepository companyRepository,
            IRolePermissionRepository rolePermissionRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IMapper mapper
        )
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
            _numberFormatRepository = numberFormatRepository;
            _cityRepository = cityRepository;
            _companyRepository = companyRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _context = context;
            _mapper = mapper;
        }

        public List<VolunteerGridDto> GetVolunteerDetail(bool isDeleted, int volunteerid)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.Id == volunteerid).
                   ProjectTo<VolunteerGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            //bool isSummary = false;
            //var volunteers = FindByInclude(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();
            //var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            //var roleBlock = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_volunteerblock");
            //var roleScreening = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_screeningEntry");
            //var roleVolunteer = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_volunteerlist");
            //var result = volunteers.Select(x => new VolunteerGridDto
            //{
            //    Id = x.Id,
            //    VolunteerNo = x.VolunteerNo,
            //    RefNo = x.RefNo,
            //    LastName = x.LastName,
            //    FirstName = x.FirstName,
            //    MiddleName = x.MiddleName,
            //    AliasName = x.AliasName,
            //    DateOfBirth = x.DateOfBirth,
            //    FromAge = x.FromAge,
            //    ToAge = x.ToAge,
            //    Religion = "",//x.Religion.ReligionName,
            //    Occupation = "",//x.Occupation.OccupationName,
            //    Education = "",//x.Education,
            //    AnnualIncome = x.AnnualIncome,
            //    Gender = x.GenderId == null ? "" : ((Gender)x.GenderId).GetDescription(),
            //    Race = "",//x.Race.RaceName,
            //    MaritalStatus = "",//x.MaritalStatus.MaritalStatusName,
            //    PopulationType = "",//x.PopulationType.PopulationName,
            //    FoodType = "",//x.FoodType.TypeName,
            //    Relationship = x.Relationship,
            //    Address = "",//x.Addresses.FirstOrDefault(a => a.IsCurrent).Location.FullAddress,
            //    ProfilePicPath = imageUrl + (x.ProfilePic ?? DocumentService.DefulatProfilePic),
            //    Foods = "",
            //    RegisterDate = x.RegisterDate,
            //    StatusName = x.Status.GetDescription(),
            //    IsDeleted = x.DeletedDate != null,
            //    Blocked = x.IsBlocked != null ? x.IsBlocked == true ? "Yes" : "No" : "No",
            //    IsBlockDisplay = roleBlock.IsView && x.IsBlocked != null,
            //    IsScreeningHisotry = roleScreening.IsView,
            //    IsDeleteRole = roleVolunteer.IsDelete,
            //    IsScreening = x.IsScreening,
            //}).OrderByDescending(x => x.Id).ToList();

            //return result;

        }

        public IList<VolunteerGridDto> GetVolunteerList()
        {
            var query = All.AsQueryable();

            var result = GetItems(query);

            return result;
        }

        public IList<VolunteerGridDto> Search(VolunteerSearchDto search)
        {
            var query = All.AsQueryable();

            if (search.Id > 0)
            {
                query = query.Where(x => x.Id == search.Id);
            }
            else
            {
                if (search.FromAge.HasValue)
                    //query = query.Where(x => x.FromAge >= search.FromAge);
                    //query = query.Where(x => x.DateOfBirth != null && (DateTime.Today.Year - x.DateOfBirth.Value.Year) >= search.FromAge);
                    query = query.Where(x => x.DateOfBirth != null && (DateTime.Today.Year -
                        (DateTime.Today.Month < x.DateOfBirth.Value.Month || (DateTime.Today.Month == x.DateOfBirth.Value.Month && DateTime.Today.Day < x.DateOfBirth.Value.Day)
                        ? x.DateOfBirth.Value.Year - 1 : x.DateOfBirth.Value.Year)) >= search.FromAge);
                if (search.ToAge.HasValue)
                    //query = query.Where(x => x.FromAge <= search.ToAge);
                    //query = query.Where(x => x.DateOfBirth != null && CalculateAge(Convert.ToDateTime(x.DateOfBirth.Value)) <= search.ToAge);
                    query = query.Where(x => x.DateOfBirth != null && (DateTime.Today.Year -
                    (DateTime.Today.Month < x.DateOfBirth.Value.Month || (DateTime.Today.Month == x.DateOfBirth.Value.Month && DateTime.Today.Day < x.DateOfBirth.Value.Day)
                    ? x.DateOfBirth.Value.Year - 1 : x.DateOfBirth.Value.Year)) <= search.ToAge);
                if (!string.IsNullOrEmpty(search.VolunteerNo))
                    query = query.Where(x =>
                        x.VolunteerNo != null && x.VolunteerNo.ToLower().Contains(search.VolunteerNo.ToLower()));
                if (!string.IsNullOrEmpty(search.FullName))
                    query = query.Where(x => x.FullName.ToLower().Contains(search.FullName.ToLower()));
                if (!string.IsNullOrEmpty(search.TextSearch))
                {
                    var volunterIds = AutoCompleteSearch(search.TextSearch.Trim());
                    IEnumerable<int> ids = volunterIds.Select(x => x.Id).Distinct();
                    query = query.Where(x => ids.Contains(x.Id));
                    //query = query.Where(x => volunterIds.Any(a => a.Id == x.Id));
                }

                if (!string.IsNullOrEmpty(search.AliasName))
                    query = query.Where(x =>
                        x.AliasName != null && x.AliasName.ToLower().Contains(search.AliasName.ToLower()));
                if (search.GenderId.HasValue)
                    query = query.Where(x => x.GenderId == search.GenderId);
                if (search.PopulationTypeId.HasValue)
                    query = query.Where(x => x.PopulationTypeId == search.PopulationTypeId);
                if (!string.IsNullOrEmpty(search.CityName))
                    query = query.Where(x =>
                        _context.VolunteerAddress.Where(t => t.Location.CityId != null &&
                                                            _context.City
                                                                .Where(c => c.CityName.ToLower()
                                                                    .Contains(search.CityName.ToLower()))
                                                                .Select(c => c.Id)
                                                                .Contains(t.Location.CityId.Value))
                            .Select(s => s.VolunteerId).Contains(x.Id));
                if (!string.IsNullOrEmpty(search.CityAreaName))
                    query = query.Where(x =>
                        _context.VolunteerAddress.Where(t => t.Location.CityAreaId != null &&
                                                            _context.CityArea
                                                                .Where(c => c.AreaName.ToLower()
                                                                    .Contains(search.CityAreaName.ToLower()))
                                                                .Select(c => c.Id)
                                                                .Contains(t.Location.CityAreaId.Value))
                            .Select(s => s.VolunteerId).Contains(x.Id));
                if (!string.IsNullOrEmpty(search.ContactNo))
                    query = query.Where(x => _context.VolunteerContact.Where(t => t.ContactNo != null &&
                                                                                 t.ContactNo.ToLower()
                                                                                     .Contains(
                                                                                         search.ContactNo.ToLower()))
                        .Select(s => s.VolunteerId).Contains(x.Id));
                if (search.Status.HasValue)
                    query = query.Where(x => x.Status == search.Status);
                if (search.IsDeleted.HasValue)
                    query = query.Where(x => search.IsDeleted == true ? x.DeletedDate != null : x.DeletedDate == null);

                if (search.IsBlocked.HasValue && search.IsBlocked == true)
                    query = query.Where(x => x.IsBlocked == true);
                if (search.IsBlocked.HasValue && search.IsBlocked == false)
                    query = query.Where(x => x.IsBlocked == null || x.IsBlocked == false);
            }

            var result = GetItems(query, true);

            return result;
        }

        public VolunteerStatusCheck CheckStatus(int id)
        {
            var volunteer = Find(id);
            if (volunteer.Status == VolunteerStatus.Completed)
                return new VolunteerStatusCheck
                { Id = id, VolunteerNo = volunteer.VolunteerNo, Status = VolunteerStatus.Completed, IsNew = false };

            var propsToCheck = new List<string>
            {
                "RegisterDate",
                "FirstName",
                "LastName",
                "DateOfBirth",
                "ReligionId",
                "OccupationId",
                "Education",
                "RaceId",
                "MaritalStatusId",
                "PopulationTypeId",
                "GenderId",
                "AnnualIncome",
                "FoodTypeId"
            };

            var inComplete = false;
            var message = "";
            foreach (var propName in propsToCheck)
            {
                var prop = volunteer.GetType().GetProperty(propName);
                var value = Convert.ToString(prop?.GetValue(volunteer));
                if (value.Trim().Length == 0)
                {
                    //return new VolunteerStatusCheck { Id = id, Status = VolunteerStatus.InCompleted, isNew = true };
                    message = "#Profile ";
                    inComplete = true;
                    break;
                }
            }
            if (!_context.VolunteerAddress.Where(t => t.VolunteerId == id).Any())
            {
                message += "#Address ";
                inComplete = true;
            }

            if (!_context.VolunteerContact.Where(t => t.VolunteerId == id).Any())
            {
                message += "#Contact ";
                inComplete = true;
            }

            //if (!_context.VolunteerFood.Where(t => t.VolunteerId == id).Any())
            //{
            //    message += "#Food ";
            //    inComplete = true;
            //}

            if (!_context.VolunteerLanguage.Where(t => t.VolunteerId == id).Any())
            {
                message += "#Language";
                inComplete = true;
            }

            if (!inComplete)
            {
                volunteer.Status = VolunteerStatus.Completed;
                volunteer.VolunteerNo = GetVolunteerNumber();

                if (_context.Volunteer.Where(t => t.VolunteerNo == volunteer.VolunteerNo).Any())
                {
                    return new VolunteerStatusCheck
                    { Id = id, Status = VolunteerStatus.InCompleted, IsNew = false, StatusName = "Volunteer Number already exists." };
                }
                else
                {
                    Update(volunteer);
                    _context.Save();
                    return new VolunteerStatusCheck
                    {
                        Id = id,
                        VolunteerNo = volunteer.VolunteerNo,
                        Status = VolunteerStatus.Completed,
                        IsNew = true,
                        StatusName = message
                    };
                }

            }

            return new VolunteerStatusCheck
            { Id = id, Status = VolunteerStatus.InCompleted, IsNew = true, StatusName = message };
        }

        public IList<DropDownDto> GetVolunteerForAttendance(VolunteerSearchDto search)
        {
            var query = All.Where(x => x.DeletedDate == null && x.Status == VolunteerStatus.Completed && (x.IsBlocked == false || x.IsBlocked == null));

            if (!string.IsNullOrEmpty(search.TextSearch))
            {
                var volunterIds = AutoCompleteSearch(search.TextSearch.Trim());
                IEnumerable<int> ids = volunterIds.Select(x => x.Id).Distinct();
                query = query.Where(x => ids.Contains(x.Id));
                //query = query.Where(x => volunterIds.Any(a => a.Id == x.Id));
            }

            if (search.PeriodNo == 1)
                query = query.Where(x => x.Attendances.Any(r => r.ScreeningEntry.IsFitnessFit == true
                                                                                          && r.ScreeningEntry.ScreeningDate.Date >=
                                                                                          DateTime.Now
                                                                                              .AddDays(-search
                                                                                                  .LastScreening)
                                                                                              .Date));

            var projectId = _context.ProjectDesignPeriod.Where(t => t.Id == search.ProjectDesignPeriodId)
                .Select(x => x.ProjectDesign.ProjectId).FirstOrDefault();

            query = query.Where(x => !_context.Attendance.Any(t => t.DeletedDate == null
                                                                  && (t.Status == null ||
                                                                      t.Status != AttendaceStatus.Suspended)
                                                                  && t.PeriodNo == search.PeriodNo &&
                                                                  t.ProjectId == projectId && t.VolunteerId == x.Id));


            return query.Select(t => new DropDownDto
            {
                Id = t.Id,
                Value = t.VolunteerNo + " " + t.FirstName + " " + t.MiddleName + " " + t.LastName
            }).ToList();
        }

        public IList<DropDownDto> AutoCompleteSearch(string searchText, bool isAutoSearch = false)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return new List<DropDownDto>();
            searchText = searchText.Trim();

            var query = All.Where(x => x.DeletedDate == null).AsQueryable();

            //query = query.Where(x => x.FullName.Contains(searchText) || x.VolunteerNo.Contains(searchText));
            query = query.Where(x => x.FirstName.Contains(searchText) || x.MiddleName.Contains(searchText) || x.LastName.Contains(searchText) || x.VolunteerNo.Contains(searchText));

            if (isAutoSearch)
                query = query.Take(7);

            return query.Select(t => new DropDownDto
            {
                Id = t.Id,
                //Value = t.VolunteerNo + " " + t.FullName
                Value = t.VolunteerNo + " " + t.FirstName + " " + t.MiddleName + " " + t.LastName
            }).ToList();
        }



        //public IList<DropDownDto> getVolunteersForDataEntryByPeriodIdLocked(int? projectDesignPeriodId, int projectId, bool isLock)
        //{
        //    var proId = _context.Project.Where(x => x.Id == projectId).FirstOrDefault().ParentProjectId ?? projectId;
        //    var projectdesignId = _context.ProjectDesign.Where(x => x.ProjectId == proId && x.DeletedDate == null).FirstOrDefault().Id;
        //    var PeriodId = _context.ProjectDesignPeriod.Where(x => x.ProjectDesignId == projectdesignId && x.DeletedDate == null).FirstOrDefault().Id;
        //    var subjects = new List<DropDownDto>();


        //    return null;
        //}

        private IList<VolunteerGridDto> GetItems(IQueryable<Data.Entities.Volunteer.Volunteer> query,
            bool isSummary = false)
        {
            var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            var roleBlock = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_volunteerblock");
            var roleScreening = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_screeningEntry");
            var roleVolunteer = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_volunteerlist");
            var result = query.Select(x => new VolunteerGridDto
            {
                Id = x.Id,
                VolunteerNo = x.VolunteerNo,
                RefNo = x.RefNo,
                LastName = x.LastName,
                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                AliasName = x.AliasName,
                DateOfBirth = x.DateOfBirth,
                FromAge = x.FromAge,
                ToAge = x.ToAge,
                Religion = x.Religion.ReligionName,
                Occupation = x.Occupation.OccupationName,
                Education = x.Education,
                AnnualIncome = x.AnnualIncome,
                Gender = x.GenderId == null ? "" : ((Gender)x.GenderId).GetDescription(),
                Race = x.Race.RaceName,
                MaritalStatus = x.MaritalStatus.MaritalStatusName,
                PopulationType = x.PopulationType.PopulationName,
                FoodType = x.FoodType.TypeName,
                Relationship = x.Relationship,
                Address = "",//x.Addresses.FirstOrDefault(a => a.IsCurrent).Location.FullAddress,
                ProfilePicPath = imageUrl + (x.ProfilePic ?? DocumentService.DefulatProfilePic),
                Foods = "",
                RegisterDate = x.RegisterDate,
                StatusName = x.Status.GetDescription(),
                IsDeleted = x.DeletedDate != null,
                Blocked = x.IsBlocked != null ? x.IsBlocked == true ? "Yes" : "No" : "No",
                IsBlockDisplay = roleBlock.IsView && x.IsBlocked != null,
                IsScreeningHisotry = roleScreening.IsView,
                IsDeleteRole = roleVolunteer.IsDelete,
                IsScreening = x.IsScreening,
                CreatedDate = x.CreatedDate,
                ModifiedDate = x.ModifiedDate,
                DeletedDate = x.DeletedDate,
                CreatedByUser = x.CreatedByUser.UserName,
                ModifiedByUser = x.ModifiedByUser.UserName,
                DeletedByUser = x.DeletedByUser.UserName,
            }).OrderByDescending(x => x.Id).ToList();

            return result;
        }

        private string GetVolunteerNumber()
        {
            var number = _numberFormatRepository.GenerateNumber("vol");
            var companyLocation = _companyRepository.Find(_jwtTokenAccesser.CompanyId).Location;
            var cityCode = "";
            if (companyLocation != null && companyLocation.CityId != null)
                cityCode = _cityRepository.Find((int)companyLocation.CityId).CityCode;


            number = number.Replace("CITY", cityCode);

            return number.ToUpper();
        }

        public IList<DropDownDto> QueryAutoCompleteSearch(string searchText, bool isAutoSearch = false)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return new List<DropDownDto>();
            searchText = searchText.Trim();

            var query = All.Where(x => x.DeletedDate == null && x.Status == VolunteerStatus.Completed).AsQueryable();

            //query = query.Where(x => x.FullName.Contains(searchText) || x.VolunteerNo.Contains(searchText));
            query = query.Where(x => x.FirstName.Contains(searchText) || x.MiddleName.Contains(searchText) || x.LastName.Contains(searchText) || x.VolunteerNo.Contains(searchText));

            if (isAutoSearch)
                query = query.Take(7);

            return query.Select(t => new DropDownDto
            {
                Id = t.Id,
                //Value = t.VolunteerNo + " " + t.FullName
                Value = t.VolunteerNo + " " + t.FirstName + " " + t.MiddleName + " " + t.LastName
            }).ToList();
        }

        public string DuplicateOldReference(VolunteerDto objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.RefNo == objSave.RefNo && x.DeletedDate == null))
                return "Duplicate Refrence Number : " + objSave.RefNo;
            return "";
        }

    }
}