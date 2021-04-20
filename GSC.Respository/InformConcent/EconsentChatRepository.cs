using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.InformConcent;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;


namespace GSC.Respository.InformConcent
{
    public class EconsentChatRepository : GenericRespository<EconsentChat>, IEconsentChatRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public EconsentChatRepository(IGSCContext context,
                                    IJwtTokenAccesser jwtTokenAccesser,
                                    IMapper mapper,
                                    IUserRepository userRepository,
                                    IUnitOfWork uow) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRepository = userRepository;
            _mapper = mapper;
            _uow = uow;
        }

        public void AllMessageRead(int senderId)
        {
            var messages = FindBy(x => x.SenderId == senderId && x.ReceiverId == _jwtTokenAccesser.UserId && x.IsRead == false).ToList();
            for (int i = 0; i < messages.Count; i++)
            {
                messages[i].IsRead = true;
                messages[i].ReadDateTIme = DateTime.Now;
                if (messages[i].IsDelivered == false)
                {
                    messages[i].IsDelivered = true;
                    messages[i].DeliveredDateTime = DateTime.Now;
                }
                Update(messages[i]);
            }
            _uow.Save();
        }

        public List<EConsentUserChatDto> GetChatUsersList()
        {
            var user = _userRepository.Find(_jwtTokenAccesser.UserId);
            var users = new List<User>();
            if (user.UserType == Shared.Generic.UserMasterUserType.Patient)
            {
                users = _userRepository.FindBy(x => x.Id != _jwtTokenAccesser.UserId && x.DeletedDate == null && x.IsLocked == false && x.UserType != Shared.Generic.UserMasterUserType.Patient &&
                        (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)).ToList();
            } else
            {
                var userids = new List<int>();
                userids = _context.Randomization.Where(x => x.PatientStatusId != Helper.ScreeningPatientStatus.Completed && x.PatientStatusId != Helper.ScreeningPatientStatus.ScreeningFailure && x.DeletedDate == null && x.UserId != null).Select(x => (int)x.UserId).ToList();
                users = _userRepository.FindBy(x => x.Id != _jwtTokenAccesser.UserId && x.DeletedDate == null && userids.Contains(x.Id) && x.IsLocked == false && x.UserType == Shared.Generic.UserMasterUserType.Patient &&
                        (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)).ToList();
            }
            var userschat = _mapper.Map<List<EConsentUserChatDto>>(users);
            var userintlist = users.Select(x => x.Id).ToList();
            var chatdata = FindBy(x => (x.SenderId == _jwtTokenAccesser.UserId || x.ReceiverId == _jwtTokenAccesser.UserId));
            for (int i = 0; i <= userschat.Count - 1; i++)
            {
                IList<int> intList = new List<int>() { userschat[i].Id, _jwtTokenAccesser.UserId };
                var chatobj = chatdata.Where(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId)).OrderBy(t => t.SendDateTime).LastOrDefault();//FindBy(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId)).OrderBy(t => t.SendDateTime).LastOrDefault();
                userschat[i].LastMessage = chatobj == null ? "" : chatobj.Message;
                userschat[i].UnReadMsgCount = chatdata.Where(x => x.SenderId == userschat[i].Id && x.IsRead == false).ToList().Count;
                if (chatobj != null)
                {
                    if (chatobj.ReceiverId == _jwtTokenAccesser.UserId)
                        userschat[i].LastMessageStatus = "";
                    else if (chatobj.IsDelivered == false)
                        userschat[i].LastMessageStatus = "S";
                    else if (chatobj.IsDelivered == true && chatobj.IsRead == false)
                        userschat[i].LastMessageStatus = "D";
                    else if (chatobj.IsRead == true)
                        userschat[i].LastMessageStatus = "R";
                }
                else
                    userschat[i].LastMessageStatus = "";
            }
            return userschat;
        }

        public List<EconsentChat> GetEconsentChat(int userId)
        {
            IList<int> intList = new List<int>() { userId,_jwtTokenAccesser.UserId };
            var data = FindBy(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId)).ToList();
            return data;
        }

        public int GetUnReadMessagecount()
        {
            var data = FindBy(x => x.ReceiverId == _jwtTokenAccesser.UserId && x.IsRead == false).ToList().Count;
            return data;
        }
    }
}
