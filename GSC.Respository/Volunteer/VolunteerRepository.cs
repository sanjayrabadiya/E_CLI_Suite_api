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
using GSC.Shared;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Volunteer
{
    public class VolunteerRepository : GenericRespository<Data.Entities.Volunteer.Volunteer, GscContext>,
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
        private readonly IUnitOfWork<GscContext> _uow;

        public VolunteerRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            INumberFormatRepository numberFormatRepository,
            IUploadSettingRepository uploadSettingRepository,
            ICityRepository cityRepository,
            ICompanyRepository companyRepository,
            IRolePermissionRepository rolePermissionRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IScreeningTemplateRepository screeningTemplateRepository
        )
            : base(uow, jwtTokenAccesser)
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
            _uow = uow;
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
                    query = query.Where(x => x.FromAge >= search.FromAge);
                if (search.ToAge.HasValue)
                    query = query.Where(x => x.FromAge <= search.ToAge);
                if (!string.IsNullOrEmpty(search.VolunteerNo))
                    query = query.Where(x =>
                        x.VolunteerNo != null && x.VolunteerNo.ToLower().Contains(search.VolunteerNo.ToLower()));
                if (!string.IsNullOrEmpty(search.FullName))
                    query = query.Where(x => x.FullName.ToLower().Contains(search.FullName.ToLower()));
                if (!string.IsNullOrEmpty(search.TextSearch))
                {
                    var volunterIds = AutoCompleteSearch(search.TextSearch.Trim());
                    query = query.Where(x => volunterIds.Any(a => a.Id == x.Id));
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
                        Context.VolunteerAddress.Where(t => t.Location.CityId != null &&
                                                            Context.City
                                                                .Where(c => c.CityName.ToLower()
                                                                    .Contains(search.CityName.ToLower()))
                                                                .Select(c => c.Id)
                                                                .Contains(t.Location.CityId.Value))
                            .Select(s => s.VolunteerId).Contains(x.Id));
                if (!string.IsNullOrEmpty(search.CityAreaName))
                    query = query.Where(x =>
                        Context.VolunteerAddress.Where(t => t.Location.CityAreaId != null &&
                                                            Context.CityArea
                                                                .Where(c => c.AreaName.ToLower()
                                                                    .Contains(search.CityAreaName.ToLower()))
                                                                .Select(c => c.Id)
                                                                .Contains(t.Location.CityAreaId.Value))
                            .Select(s => s.VolunteerId).Contains(x.Id));
                if (!string.IsNullOrEmpty(search.ContactNo))
                    query = query.Where(x => Context.VolunteerContact.Where(t => t.ContactNo != null &&
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
                "AnnualIncome"
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
            if (!Context.VolunteerAddress.Where(t => t.VolunteerId == id).Any())
            {
                message += "#Address ";
                inComplete = true;
            }

            if (!Context.VolunteerContact.Where(t => t.VolunteerId == id).Any())
            {
                message += "#Contact ";
                inComplete = true;
            }

            if (!Context.VolunteerFood.Where(t => t.VolunteerId == id).Any())
            {
                message += "#Food ";
                inComplete = true;
            }

            if (!Context.VolunteerLanguage.Where(t => t.VolunteerId == id).Any())
            {
                message += "#Language";
                inComplete = true;
            }

            if (!inComplete)
            {
                volunteer.Status = VolunteerStatus.Completed;
                volunteer.VolunteerNo = GetVolunteerNumber();
                Update(volunteer);
                _uow.Save();
                return new VolunteerStatusCheck
                {
                    Id = id,
                    VolunteerNo = volunteer.VolunteerNo,
                    Status = VolunteerStatus.Completed,
                    IsNew = true,
                    StatusName = message
                };
            }

            return new VolunteerStatusCheck
            { Id = id, Status = VolunteerStatus.InCompleted, IsNew = true, StatusName = message };
        }

        public IList<VolunteerAttendaceDto> GetVolunteerForAttendance(VolunteerSearchDto search)
        {
            var query = All.Where(x => x.DeletedDate == null && x.Status == VolunteerStatus.Completed);

            if (!string.IsNullOrEmpty(search.TextSearch))
            {
                var volunterIds = AutoCompleteSearch(search.TextSearch.Trim());
                query = query.Where(x => volunterIds.Any(a => a.Id == x.Id));
            }

            if (search.PeriodNo == 1)
                query = query.Where(x => x.Attendances.Any(r => r.ScreeningEntry.IsFitnessFit == true
                                                                                          && r.ScreeningEntry.ScreeningDate.Date >=
                                                                                          DateTime.Now
                                                                                              .AddDays(-search
                                                                                                  .LastScreening)
                                                                                              .Date));

            var projectId = Context.ProjectDesignPeriod.Where(t => t.Id == search.ProjectDesignPeriodId)
                .Select(x => x.ProjectDesign.ProjectId).FirstOrDefault();

            query = query.Where(x => !Context.Attendance.Any(t => t.DeletedDate == null
                                                                  && (t.Status == null ||
                                                                      t.Status != AttendaceStatus.Suspended)
                                                                  && t.PeriodNo == search.PeriodNo &&
                                                                  t.ProjectId == projectId && t.VolunteerId == x.Id));


            return query.Select(x => new VolunteerAttendaceDto
            {
                Id = x.Id,
                VolunteerNo = x.VolunteerNo,
                RefNo = x.RefNo,
                LastName = x.LastName,
                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                AliasName = x.AliasName,
                DateOfBirth = x.DateOfBirth,
                FullName = x.FullName,
                FromAge = x.FromAge,
                ToAge = x.ToAge,
                Gender = x.GenderId == null ? "" : ((Gender)x.GenderId).GetDescription(),
                Race = x.Race.RaceName,
                IsDeleted = x.DeletedDate != null,
                Blocked = x.IsBlocked ?? false
            }).OrderByDescending(x => x.FullName).ToList();
        }

        public IList<DropDownDto> AutoCompleteSearch(string searchText, bool isAutoSearch = false)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return new List<DropDownDto>();
            searchText = searchText.Trim();

            var query = All.Where(x => x.DeletedDate == null).AsQueryable();

            query = query.Where(x => x.FullName.Contains(searchText) || x.VolunteerNo.Contains(searchText));

            if (isAutoSearch)
                query = query.Take(7);

            return query.Select(t => new DropDownDto
            {
                Id = t.Id,
                Value = t.VolunteerNo + " " + t.FullName
            }).ToList();
        }

        

        public IList<DropDownDto> getVolunteersForDataEntryByPeriodIdLocked(int? projectDesignPeriodId, int projectId, bool isLock)
        {
            var proId = Context.Project.Where(x => x.Id == projectId).FirstOrDefault().ParentProjectId ?? projectId;
            var projectdesignId = Context.ProjectDesign.Where(x => x.ProjectId == proId && x.DeletedDate == null).FirstOrDefault().Id;
            var PeriodId = Context.ProjectDesignPeriod.Where(x => x.ProjectDesignId == projectdesignId && x.DeletedDate == null).FirstOrDefault().Id;
            var subjects = new List<DropDownDto>();
            //if (isLock)
            //{
            //    var attendance = Context.Attendance.Where(a => a.DeletedDate == null
            //                                                   && !a.IsProcessed &&
            //                                                   a.ProjectDesignPeriodId == PeriodId
            //                                                   && a.ProjectId == projectId
            //                                                   && a.AttendanceType != AttendanceType.Screening
            //                                                  ).Select(x =>
            //        new DropDownDto
            //        {
            //            Id = x.Id,
            //            Value = x.Volunteer == null
            //                ? Convert.ToString(x.Randomization.ScreeningNumber + " - " + x.Randomization.Initial +
            //                                   (x.Randomization.RandomizationNumber == null
            //                                       ? ""
            //                                       : " - " + x.Randomization.RandomizationNumber))
            //                : Convert.ToString(Convert.ToString(x.ProjectSubject != null ? x.ProjectSubject.Number : "") +
            //                                   " - " + x.Volunteer.FullName),
            //            Code = "Attendance",
            //            ExtraData = x.Id
            //        }).ToList();

            //    var screeningEntry = Context.ScreeningEntry.Where(a => a.DeletedDate == null
            //                                                           && a.ProjectDesignPeriodId == PeriodId
            //                                                           && a.ProjectId == projectId
            //                                                           && a.EntryType != AttendanceType.Screening).Select(
            //        x => new DropDownDto
            //        {
            //            Id = x.Id,
            //            Value = x.Attendance.Volunteer == null
            //                ? Convert.ToString(x.Attendance.Randomization.ScreeningNumber + " - " +
            //                                   x.Attendance.Randomization.Initial +
            //                                   (x.Attendance.Randomization.RandomizationNumber == null
            //                                       ? ""
            //                                       : " - " + x.Attendance.Randomization.RandomizationNumber))
            //                : Convert.ToString(
            //                    Convert.ToString(x.Attendance.ProjectSubject != null
            //                        ? x.Attendance.ProjectSubject.Number
            //                        : "") + " - " + x.Attendance.Volunteer.FullName),
            //            Code = "Screening",
            //            ExtraData = x.AttendanceId
            //        }).Distinct().ToList();
            //    subjects.AddRange(attendance);
            //    subjects.AddRange(screeningEntry);

            //    var lstsubjects = new List<DropDownDto>();
            //    lstsubjects.AddRange(attendance);
            //    lstsubjects.AddRange(screeningEntry);

            //    foreach (var item in lstsubjects)
            //    {
            //        var screeningTemplate = _screeningTemplateRepository.FindByInclude(x => x.ScreeningVisit.ScreeningEntry.AttendanceId == (int)item.ExtraData && x.DeletedDate == null).ToList();
            //        if (screeningTemplate.Count() <= 0 || screeningTemplate.Any(y => y.IsLocked == false))
            //        {
            //            var itemexist = subjects.Where(x => x.Id == item.Id).FirstOrDefault();
            //            if (itemexist == null)
            //            {
            //                subjects.Add(item);
            //            }
            //        }
            //        else
            //        {
            //            subjects.RemoveAll(x => x.Id == item.Id);
            //        }
            //    }
            //}
            //else
            //{
            //    var attendance = (from atten in Context.Attendance.Where(a => a.DeletedDate == null
            //                                                  && !a.IsProcessed &&
            //                                                  a.ProjectDesignPeriodId == PeriodId
            //                                                  && a.ProjectId == projectId
            //                                                  && a.AttendanceType != AttendanceType.Screening)
            //                      join locktemplate in Context.ScreeningTemplate.Where(x => x.IsLocked)
            //                      on atten.Id equals locktemplate.ScreeningVisit.ScreeningEntry.AttendanceId
            //                      select new DropDownDto
            //                      {
            //                          Id = atten.Id,
            //                          Value = atten.Volunteer == null
            //                              ? Convert.ToString(atten.Randomization.ScreeningNumber + " - " + atten.Randomization.Initial +
            //                                                 (atten.Randomization.RandomizationNumber == null
            //                                                     ? ""
            //                                                     : " - " + atten.Randomization.RandomizationNumber))
            //                              : Convert.ToString(Convert.ToString(atten.ProjectSubject != null ? atten.ProjectSubject.Number : "") +
            //                                                 " - " + atten.Volunteer.FullName),
            //                          Code = "Attendance",
            //                          ExtraData = atten.Id
            //                      }).ToList();

            //    var screeningEntry = (from screening in Context.ScreeningEntry.Where(a => a.DeletedDate == null
            //                                                           && a.ProjectDesignPeriodId == PeriodId
            //                                                           && a.ProjectId == projectId
            //                                                           && a.EntryType != AttendanceType.Screening)
            //                          join locktemplate in Context.ScreeningTemplate.Where(x => x.IsLocked)
            //                          on screening.AttendanceId equals locktemplate.ScreeningVisit.ScreeningEntry.AttendanceId
            //                          select new DropDownDto
            //                          {
            //                              Id = screening.Id,
            //                              Value = screening.Attendance.Volunteer == null
            //                                ? Convert.ToString(screening.Attendance.Randomization.ScreeningNumber + " - " +
            //                                                   screening.Attendance.Randomization.Initial +
            //                                                   (screening.Attendance.Randomization.RandomizationNumber == null
            //                                                       ? ""
            //                                                       : " - " + screening.Attendance.Randomization.RandomizationNumber))
            //                                : Convert.ToString(
            //                                    Convert.ToString(screening.Attendance.ProjectSubject != null
            //                                        ? screening.Attendance.ProjectSubject.Number
            //                                        : "") + " - " + screening.Attendance.Volunteer.FullName),
            //                              Code = "Screening",
            //                              ExtraData = screening.AttendanceId
            //                          }).Distinct().ToList();
            //    subjects.AddRange(attendance);
            //    subjects.AddRange(screeningEntry);
            //}

            //var volunteer = subjects.GroupBy(x => x.Id).Select(s => new DropDownDto()
            //{
            //    Id = s.Key,
            //    Value = s.FirstOrDefault().Value,
            //    ExtraData = s.FirstOrDefault().ExtraData
            //}).ToList();

            //return volunteer;

            return null;
        }

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
                Relationship = x.Relationship,
                Address = x.Addresses.FirstOrDefault(a => a.IsCurrent).Location.FullAddress,
                ProfilePicPath = imageUrl + (x.ProfilePic ?? DocumentService.DefulatProfilePic),
                Foods = !isSummary
                     ? ""
                     : string.Join(", ",
                         Context.FoodType
                             .Where(t => Context.VolunteerFood.Where(v => v.VolunteerId == x.Id)
                                 .Select(s => s.FoodTypeId).Contains(t.Id)).Select(s => s.TypeName).ToList()),
                RegisterDate = x.RegisterDate,
                StatusName = x.Status.GetDescription(),
                IsDeleted = x.DeletedDate != null,
                Blocked = roleBlock.IsAdd && x.IsBlocked != null ? x.IsBlocked == true ? "Yes" :
                     "No" :
                     roleBlock.IsAdd ? "No" : "",
                IsBlockDisplay = roleBlock.IsView && x.IsBlocked != null,
                IsScreeningHisotry = roleScreening.IsView,
                IsDeleteRole = roleVolunteer.IsDelete,
                IsScreening = x.IsScreening,
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
    }
}