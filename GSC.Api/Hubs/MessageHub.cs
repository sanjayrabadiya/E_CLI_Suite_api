using GSC.Common.UnitOfWork;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.InformConcent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Hubs
{
    public class MessageHub : Hub
    {
        public async Task NewMessage(EconsentChat msg)
        {
            // ------------ send to multiple -----------------
            //var userlist = ConnectedUser.Ids.Where(x => x.userId == msg.ReceiverId).ToList();
            //List<string> listData = new List<string>();
            //for (int i = 0; i < userlist.Count; i++)
            //{
            //    listData.Add(userlist[i].connectionId);
            //}
            // IReadOnlyList<string> readOnlyData = listData.AsReadOnly();
            //await Clients.Clients(readOnlyData).SendAsync("NewMessage", msg);

            // ------------ send to single -----------------
            if (ConnectedUser.Ids.Where(x => x.userId == msg.ReceiverId).ToList().Count > 0)
            {
                var connectionId = ConnectedUser.Ids.Where(x => x.userId == msg.ReceiverId).ToList().FirstOrDefault().connectionId;
                await Clients.Client(connectionId).SendAsync("NewMessage", msg);
            }
            
        }

        public async Task MessageDelivered(EconsentChat msg)
        {
            var connectionId = ConnectedUser.Ids.Where(x => x.userId == msg.SenderId).ToList().FirstOrDefault().connectionId;
            await Clients.Client(connectionId).SendAsync("MessageDelivered", msg);
        }

        public async Task AllMessageDelivered(List<int> senderIds)
        {
            var userlist = ConnectedUser.Ids.Where(x => senderIds.Contains(x.userId)).ToList();
            List<string> listData = new List<string>();
            for (int i = 0; i < userlist.Count; i++)
            {
                listData.Add(userlist[i].connectionId);
            }
            IReadOnlyList<string> readOnlyData = listData.AsReadOnly();
            await Clients.Clients(readOnlyData).SendAsync("AllMessageDelivered", "Done");
        }

        public void RemoveUser()
        {
            var user = ConnectedUser.Ids.Where(x => x.connectionId == Context.ConnectionId).ToList().FirstOrDefault();
            ConnectedUser.Ids.Remove(user);
        }

        public override async Task OnConnectedAsync()
        {
            var value = Context.GetHttpContext().Request.Query["userid"].FirstOrDefault().ToString();
            SignalRUser signalRUser = new SignalRUser();
            signalRUser.connectionId = Context.ConnectionId;
            signalRUser.userId = Convert.ToInt32(value);
            ConnectedUser.Ids.Add(signalRUser);
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("UserLogIn", Convert.ToInt32(value));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = ConnectedUser.Ids.Where(x => x.connectionId == Context.ConnectionId).ToList().FirstOrDefault();
            ConnectedUser.Ids.Remove(user);
            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("UserLogOut", Convert.ToInt32(user.userId));
        }

    }

    public static class ConnectedUser
    {
        public static List<SignalRUser> Ids = new List<SignalRUser>();
    }

    public class SignalRUser
    {
        public int userId { get; set; }
        public string connectionId { get; set; }
    }
}
