using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.InformConcent;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.UserMgt;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public class EconsentChatRepository : GenericRespository<EconsentChat, GscContext>, IEconsentChatRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public EconsentChatRepository(IUnitOfWork<GscContext> uow,
                                    IJwtTokenAccesser jwtTokenAccesser,
                                    IMapper mapper,
                                    IUserRepository userRepository) : base(uow, jwtTokenAccesser)
        {
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public List<EConsentUserChatDto> GetChatUsersList()
        {
            var users = _userRepository.FindBy(x => x.Id != _jwtTokenAccesser.UserId && x.DeletedDate == null && x.IsLocked == false).ToList();
            var userschat = _mapper.Map<List<EConsentUserChatDto>>(users);
            userschat.ForEach(e =>
            {
                IList<int> intList = new List<int>() { e.Id, _jwtTokenAccesser.UserId };
                var chatobj = FindBy(x => intList.Contains(x.SenderId) && intList.Contains(x.ReceiverId)).ToList().OrderBy(t => t.SendDateTime).ToList().LastOrDefault();
                e.LastMessage = chatobj == null ? "" : chatobj.Message;
                e.UnReadMsgCount = FindBy(x => x.SenderId == e.Id && x.IsRead == false).ToList().Count;
                if (chatobj != null)
                {
                    if (chatobj.ReceiverId == _jwtTokenAccesser.UserId)
                    {
                        e.LastMessageStatus = "";
                    }
                    else if (chatobj.IsDelivered == false)
                    {
                        e.LastMessageStatus = "S";
                    }
                    else if (chatobj.IsDelivered == true && chatobj.IsRead == false)
                    {
                        e.LastMessageStatus = "D";
                    }
                    else if (chatobj.IsRead == true)
                    {
                        e.LastMessageStatus = "R";
                    }
                }
                else e.LastMessageStatus = "";
                
            });
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
