using GSC.Data.Entities.InformConcent;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace GSC.Api.Hubs
{
    [AllowAnonymous]
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

        public void RemoveUser()
        {
            var user = ConnectedUser.Ids.Where(x => x.connectionId == Context.ConnectionId).ToList().FirstOrDefault();
            ConnectedUser.Ids.Remove(user);
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                if (Context.GetHttpContext().Request.Query["userid"].FirstOrDefault() != null)
                {
                    var value = Context.GetHttpContext().Request.Query["userid"].FirstOrDefault().ToString();
                    SignalRUser signalRUser = new SignalRUser();
                    signalRUser.connectionId = Context.ConnectionId;
                    signalRUser.userId = Convert.ToInt32(value);
                    ConnectedUser.Ids.Add(signalRUser);
                    await base.OnConnectedAsync();
                    await Clients.All.SendAsync("UserLogIn", Convert.ToInt32(value));
                }
            } catch (Exception)
            {

            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var user = ConnectedUser.Ids.Where(x => x.connectionId == Context.ConnectionId).ToList().FirstOrDefault();
                await base.OnDisconnectedAsync(exception);
                if (user != null)
                {
                    ConnectedUser.Ids.Remove(user);
                    await Clients.All.SendAsync("UserLogOut", Convert.ToInt32(user.userId));
                }
            }
            catch(Exception) { }
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
