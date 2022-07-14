using ezCloud.SignalR.Common;
using ezCloud.SignalR.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace ezCloud.SignalR.Hubs
{
    public class SignalRHub : Hub
    {
        private IConfiguration _configuration;
        private string _connectionInfo;
        private RedisClient _redisClient;
        private string _userSession;

        public SignalRHub(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionInfo = _configuration["Redis:ConnectionInfo"];
            _userSession = _configuration["Redis:UserSession"];
            _redisClient = new RedisClient(_configuration);   
        }

        public async Task SendMessage(int hotelId, string sender, string message)
        {
            try
            {
                await Clients.Group(hotelId.ToString()).SendAsync("ReceiveMessage", sender, message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public async Task ChangeHotel(int newHotel)
        {
            var currentHotel = _redisClient.HashGet(_connectionInfo, Context.ConnectionId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentHotel);

            _redisClient.HashSet(_connectionInfo, Context.ConnectionId, newHotel.ToString());
            await Groups.AddToGroupAsync(Context.ConnectionId, newHotel.ToString());
        }

        public override Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                if (httpContext == null)
                {
                    
                    return Task.FromException(new Exception("Http Context was null"));   
                }
                var token = httpContext.Request.Query["token"];
                var hotelId = httpContext.Request.Query["hotelId"];

                if (token == "")
                {
                    Clients.Caller.SendAsync("HubMessage", "No access_token");
                    return Task.FromException(new Exception("No access token avaiable"));
                }

                if(!IsTokenValid(token))
                {
                    Clients.Caller.SendAsync("HubMessage", "Access token invalid");
                    return Task.FromException(new Exception("Access token invalid"));
                }
                

                _redisClient.HashSet(_connectionInfo,Context.ConnectionId,hotelId);

                Groups.AddToGroupAsync(Context.ConnectionId, hotelId.ToString());
                Clients.Caller.SendAsync("HubMessage", "Connection success");
                return base.OnConnectedAsync();
            }
            catch(Exception e)
            {
                return Task.FromException(e);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string groupJoined = _redisClient.HashGet(_connectionInfo, Context.ConnectionId);

            _redisClient.HashDelete(_connectionInfo, Context.ConnectionId);
            Groups.RemoveFromGroupAsync(Context.ConnectionId, groupJoined);

            return base.OnDisconnectedAsync(exception);
        }

        public bool IsTokenValid(string token)
        {
            try
            {
                string userSession = _redisClient.HashGet(_userSession, token);

                if(string.IsNullOrEmpty(userSession))
                {
                    return false;
                }

                var session = JsonConvert.DeserializeObject<UserSessionInfoModel>(userSession);

                if(session == null)
                {
                    return false;
                }

                if(session.ExpirationDateTime < DateTime.Now)
                {
                    return false;
                }

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

    }
}
