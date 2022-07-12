using ezCloud.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;

namespace ezCloud.SignalR
{
    public class RabbitMQReceiverService : BackgroundService
    {
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        private string _hostName;
        private string _queueName;
        private int _port;
        private string _user;
        private string _password;
        IHubContext<SignalRHub> _hubContext;
        public RabbitMQReceiverService(IConfiguration configuration, IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
            _hostName = configuration["RabbitMQ:HostName"];
            _queueName = configuration["RabbitMQ:QueueName"];
            _port = Int32.Parse(configuration["RabbitMQ:Port"]);
            _user = configuration["RabbitMQ:User"];
            _password = configuration["RabbitMQ:Password"];

            _factory = new ConnectionFactory() { HostName = _hostName, Port = _port, UserName = _user, Password = _password };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                _channel.Dispose();
                _connection.Dispose();

                return Task.CompletedTask;
            }

            _channel.QueueDeclare(queue: _queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(message);
                var args = JsonConvert.DeserializeObject<MessageArgs>(message);
                if (args != null)
                {
                    _hubContext.Clients.Group(args.HotelId).SendAsync("ReceiveMessage", args.Sender, args.Message);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }
    }

    public class MessageArgs
    {
        [JsonProperty("hotelId")]
        public string? HotelId { get; set; }
        [JsonProperty("sender")]
        public string? Sender { get; set; }
        [JsonProperty("message")]
        public string? Message { get; set; }
    }
}
