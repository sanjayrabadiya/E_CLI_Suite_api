using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GSC.Respository.Volunteer
{
    public class VolunteerQueryRepository : GenericRespository<Data.Entities.Volunteer.VolunteerQuery>, IVolunteerQueryRepository
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;

        public VolunteerQueryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper
        )
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
        }

        public VolunteerQuery GetLatest(int VolunteerId, string FiledName)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null && x.VolunteerId == VolunteerId && x.FieldName == FiledName && x.QueryStatus != CommentStatus.SelfCorrection).OrderByDescending(o => o.Id).FirstOrDefault();
        }

        public IList<VolunteerQueryDto> GetData(int volunteerid)
        {
            var queryDtos =
                      (from query in _context.VolunteerQuery.Where(t => t.VolunteerId == volunteerid)
                       join reasonTemp in _context.AuditReason on query.ReasonId equals reasonTemp.Id into reasonDt
                       from reason in reasonDt.DefaultIfEmpty()
                       join userTemp in _context.Users on query.CreatedBy equals userTemp.Id into userDto
                       from user in userDto.DefaultIfEmpty()
                       join roleTemp in _context.SecurityRole on query.UserRole equals roleTemp.Id into roleDto
                       from role in roleDto.DefaultIfEmpty()
                       select new VolunteerQueryDto
                       {
                           Id = query.Id,
                           TableId = query.TableId,
                           FieldName = query.FieldName,
                           ReasonName = reason.ReasonName,
                           ReasonOth = query.ReasonOth,
                           Comment = query.Comment,
                           VolunteerId = query.VolunteerId,
                           CreatedDate = query.CreatedDate,
                           CreatedByName = role == null || string.IsNullOrEmpty(role.RoleName)
                               ? user.UserName
                               : user.UserName + "(" + role.RoleName + ")",
                           StatusName = query.QueryStatus.GetDescription(),
                           QueryStatus = query.QueryStatus,
                       }).OrderByDescending(o => o.Id).ToList();



            foreach (var item in queryDtos)
            {
                ButtonQueryShow button = new ButtonQueryShow();
                var commentLatest = All.Where(t => t.VolunteerId == volunteerid && t.FieldName == item.FieldName).OrderByDescending(o => o.Id).FirstOrDefault();

                if (commentLatest == null)
                {
                    button.ShowEditButton = true;
                    button.ShowRespondButton = false;
                }
                else
                {
                    if (item.Id == commentLatest.Id)
                    {
                        if (commentLatest.QueryStatus == CommentStatus.Open && item.QueryStatus == CommentStatus.Open)
                        {
                            button.ShowEditButton = true;
                            button.ShowRespondButton = true;
                        }
                        else if (commentLatest.QueryStatus == CommentStatus.Answered || commentLatest.QueryStatus == CommentStatus.Resolved)
                        {
                            var commentLatestQuery = All.Where(t => t.VolunteerId == volunteerid && t.FieldName == item.FieldName && t.QueryStatus == CommentStatus.Open).OrderByDescending(o => o.Id).FirstOrDefault();

                            if (commentLatestQuery == null)
                            {
                                button.ShowEditButton = false;
                                button.ShowRespondButton = false;
                            }
                            else if (_jwtTokenAccesser.RoleId == commentLatestQuery.UserRole)
                            {
                                button.ShowEditButton = false;
                                button.ShowRespondButton = true;
                            }
                            else
                            {
                                button.ShowEditButton = false;
                                button.ShowRespondButton = false;
                            }
                        }
                        else if (commentLatest.QueryStatus == CommentStatus.Open && _jwtTokenAccesser.RoleId != commentLatest.UserRole)
                        {
                            button.ShowEditButton = false;
                            button.ShowRespondButton = false;
                        }
                        else if (commentLatest.QueryStatus == CommentStatus.SelfCorrection || commentLatest.QueryStatus == CommentStatus.Closed)
                        {
                            button.ShowEditButton = false;
                            button.ShowRespondButton = false;
                        }
                        else
                        {
                            button.ShowEditButton = false;
                            button.ShowRespondButton = false;
                        }
                    }
                    else
                    {
                        button.ShowEditButton = false;
                        button.ShowRespondButton = false;
                    }

                }

                item.ShowButton = button;
                item.LatestFieldName = commentLatest.FieldName;
            }

            return queryDtos;
        }

        public IList<VolunteerQueryDto> VolunteerQuerySearch(VolunteerQuerySearchDto search)
        {
            var queryz = (from p in _context.VolunteerQuery
                          group p by new
                          {
                              p.VolunteerId,
                              p.FieldName
                          } into g
                          select new
                          {
                              VolunteerId = g.Key.VolunteerId,
                              FieldName = g.Key.FieldName,
                              Id = g.Select(m => m.Id).Max()
                          }).ToList();

            HashSet<int> QueryIDs = new HashSet<int>(queryz.Select(s => s.Id));

            var query1 =
                (from query in _context.VolunteerQuery
                 join volunteers in _context.Volunteer on query.VolunteerId equals volunteers.Id into volDt
                 from vol in volDt.DefaultIfEmpty()
                 join reasonTemp in _context.AuditReason on query.ReasonId equals reasonTemp.Id into reasonDt
                 from reason in reasonDt.DefaultIfEmpty()
                 join userTemp in _context.Users on query.CreatedBy equals userTemp.Id into userDto
                 from user in userDto.DefaultIfEmpty()
                 join roleTemp in _context.SecurityRole on query.UserRole equals roleTemp.Id into roleDto
                 from role in roleDto.DefaultIfEmpty()
                 where QueryIDs.Contains(query.Id)
                 select new VolunteerQueryDto
                 {
                     Id = query.Id,
                     TableId = query.TableId,
                     FieldName = query.FieldName,
                     ReasonName = reason.ReasonName,
                     ReasonOth = query.ReasonOth,
                     Comment = query.Comment,
                     VolunteerId = query.VolunteerId,
                     CreatedDate = query.CreatedDate,
                     CreatedByName = role == null || string.IsNullOrEmpty(role.RoleName)
                                                                ? user.UserName
                                                                : user.UserName + "(" + role.RoleName + ")",
                     StatusName = query.QueryStatus.GetDescription(),
                     QueryStatus = query.QueryStatus,
                     VolunteerNo = vol.VolunteerNo,
                     CreatedBy = (int)query.CreatedBy,
                     UserRole = query.UserRole
                 }).OrderByDescending(x => x.Id).ToList();

            if (search.Status.HasValue)
                query1 = query1.Where(x => x.QueryStatus == search.Status).ToList();

            if(search.User.HasValue)
                query1 = query1.Where(x => x.CreatedBy == search.User).ToList();

            if (search.Role.HasValue)
                query1 = query1.Where(x => x.UserRole == search.Role).ToList();

            if (search.FromRegistration.HasValue || search.ToRegistration.HasValue)
            {
                if (search.FromRegistration.HasValue && search.ToRegistration.HasValue)
                {
                    query1 = query1.Where(x => x.CreatedDate >= search.FromRegistration && x.CreatedDate <= search.ToRegistration).ToList();
                }
                else if (search.FromRegistration.HasValue)
                {
                    query1 = query1.Where(x => x.CreatedDate >= search.FromRegistration).ToList();
                }
                else if (search.ToRegistration.HasValue)
                {
                    query1 = query1.Where(x => x.CreatedDate <= search.ToRegistration).ToList();
                }
            }

            return query1;
        }

        public List<VolunteerQuery> GetDetailsByVolunteerId(int VolunteerId)
        {
            var queries = All.Where(x => x.VolunteerId == VolunteerId)
                .GroupBy(x => new { x.FieldName, x.VolunteerId })
                .Select(y => new VolunteerQueryDto
                {
                    VolunteerId = y.Key.VolunteerId,
                    FieldName = y.Key.FieldName,
                    Id = y.Select(m => m.Id).Max()
                });

            var result = All.Where(x => x.VolunteerId == VolunteerId && queries.Select(y => y.Id).Contains(x.Id) && x.QueryStatus == CommentStatus.Open).ToList();
            return result;
        }

        public List<DropDownDto> GetUsers()
        {
            var queries = All.GroupBy(x => x.CreatedBy).Select(y => new DropDownDto
            {
                Id = y.Select(m => m.Id).Max(),
                ExtraData = y.Key
            }).ToList();

            var result = All.Where(x => queries.Select(y => y.Id).Contains(x.Id))
                        .Select(y => new DropDownDto
                        {
                            Id = (int)y.CreatedBy,
                            Value = y.CreatedByUser.UserName
                        }).ToList();
            return result;
        }

        public List<DropDownDto> GetRoles()
        {
            var queries = All.GroupBy(x => x.UserRole).Select(y => new DropDownDto
            {
                Id = y.Select(m => m.Id).Max(),
                ExtraData = y.Key
            }).ToList();

            var result = (from query in _context.VolunteerQuery.Where(t => queries.Select(y => y.Id).Contains(t.Id))
                          join roleTemp in _context.SecurityRole on query.UserRole equals roleTemp.Id into roleDto
                          from role in roleDto.DefaultIfEmpty()
                          select new DropDownDto
                          {
                              Id = query.UserRole,
                              Value = role.RoleShortName
                          }).ToList();
            return result;
        }
    }
}
