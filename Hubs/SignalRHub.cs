using ezCloud.SignalR.Common;
using Microsoft.AspNetCore.SignalR;

namespace ezCloud.SignalR.Hubs
{
    public class SignalRHub : Hub
    {
        private IConfiguration _configuration;
        private string _connectionInfo;
        private RedisClient _redisClient;
        public SignalRHub(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionInfo = _configuration["Redis:ConnectionInfo"];
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
                if(token == "")
                {
                    Clients.Caller.SendAsync("HubMessage", "No access_token");
                    return Task.FromException(new Exception("No access token avaiable"));
                }    
                var hotelId = httpContext.Request.Query["hotelId"];

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

    }
}
