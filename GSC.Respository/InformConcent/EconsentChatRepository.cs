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
using GSC.Shared.Security;
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
                var projectId = _context.Randomization.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.UserId != null).Select(x => x.ProjectId).FirstOrDefault();
                var medicalteam = _context.SiteTeam.Where(x => x.ProjectId == projectId && x.DeletedDate == null).Select(x => x.UserId).ToList();
                users = _userRepository.FindBy(x => medicalteam.Contains(x.Id) && x.DeletedDate == null && x.UserType != Shared.Generic.UserMasterUserType.Patient).ToList();


            }
            else
            {
                var projectlist = _context.ProjectRight.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null).Select(t => t.ProjectId).ToList();
                var patientlist = _context.Randomization.Where(r => projectlist.Contains(r.ProjectId) && r.UserId != null && r.DeletedDate == null).Select(x => (int)x.UserId).ToList();
                users = _userRepository.FindBy(x => x.Id != _jwtTokenAccesser.UserId && x.DeletedDate == null && patientlist.Contains(x.Id) && x.UserType == Shared.Generic.UserMasterUserType.Patient).ToList();
            }
            var userschat = _mapper.Map<List<EConsentUserChatDto>>(users);
            var userintlist = users.Select(x => x.Id).ToList();
            var chatdata = FindBy(x => (x.SenderId == _jwtTokenAccesser.UserId || x.ReceiverId == _jwtTokenAccesser.UserId));
            userschat.ForEach(ch =>
            {


                // {
                IList<int> intList = new List<int>() { ch.Id, _jwtTokenAccesser.UserId };
                var chatobj = chatdata.Where(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId)).OrderBy(t => t.SendDateTime).LastOrDefault();//FindBy(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId)).OrderBy(t => t.SendDateTime).LastOrDefault();
                ch.LastMessage = chatobj == null ? "" : EncryptionDecryption.DecryptString(chatobj.Salt, chatobj.Message);
                ch.SendDateTime = chatobj?.SendDateTime==null?"": Convert.ToDateTime(chatobj?.SendDateTime).ToString("yyyy-MM-ddTHH:mm:ss");
                ch.UnReadMsgCount = chatdata.Where(x => x.SenderId == ch.Id && x.IsRead == false).ToList().Count;
                if (chatobj != null)
                {
                    if (chatobj.ReceiverId == _jwtTokenAccesser.UserId)
                        ch.LastMessageStatus = "";
                    else if (chatobj.IsDelivered == false)
                        ch.LastMessageStatus = "S";
                    else if (chatobj.IsDelivered == true && chatobj.IsRead == false)
                        ch.LastMessageStatus = "D";
                    else if (chatobj.IsRead == true)
                        ch.LastMessageStatus = "R";
                }
                else
                    ch.LastMessageStatus = "";
                //}
            });
            return userschat.OrderByDescending(x => x.SendDateTime).ToList();
        }
        public List<EconsentChatDto> GetEconsentChat(int userId)
        {
            IList<int> intList = new List<int>() { userId, _jwtTokenAccesser.UserId };
            var data = FindBy(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId)).ToList();
            data.ForEach(x => x.Message = EncryptionDecryption.DecryptString(x.Salt, x.Message));
            var result = _mapper.Map<List<EconsentChatDto>>(data);
            return result;
        }

        public EconsentChatDetailDto GetEconsentChat(EconcentChatParameterDto details)
        {
            IList<int> intList = new List<int>() { details.UserId, _jwtTokenAccesser.UserId };
            //var data = FindBy(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId) && details.LastDate!=null ? x.SendDateTime <= details.LastDate : x.SendDateTime <= DateTime.Now && x.Message.Contains(details.SearchString)).Take(details.PageSize).OrderBy(x=>x.SendDateTime).ToList();
            //var data = FindBy(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId) && (details.LastDate != null ? x.SendDateTime <= details.LastDate : x.SendDateTime <= DateTime.Now)).Take(details.PageSize).OrderBy(x => x.SendDateTime).ToList();
            var data = FindBy(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId)).Take(details.PageSize).OrderBy(x => x.SendDateTime).Skip(10 * (details.PageNumber - 1)).Take(10).ToList();
            int totalRecord= FindBy(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId)).Count();
            data.ForEach(x => x.Message = EncryptionDecryption.DecryptString(x.Salt, x.Message));            
            var result = _mapper.Map<List<EconsentChatDto>>(data);
            EconsentChatDetailDto _details = new EconsentChatDetailDto();
            _details.ChatDetails = result;
            _details.PageNumber = details.PageNumber;
            _details.PageSize = details.PageSize;
            _details.TotalRecord = totalRecord;
            return _details;
        }

        public int GetUnReadMessagecount()
        {
            var data = FindBy(x => x.ReceiverId == _jwtTokenAccesser.UserId && x.IsRead == false).ToList().Count;
            return data;
        }
    }
}
