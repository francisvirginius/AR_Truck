using MQTTnet;
using MQTTnet.Extensions.TopicTemplate;
using System.Text;
using TDEV_811.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TDEV_811.Services
{
    public class MqttHostedService : BackgroundService
    {
        private readonly ILogger<MqttHostedService> _logger;
        private IMqttClient _mqttClient;
        readonly MqttTopicTemplate topicTemplate = new("arduino/coucou");

        public MqttHostedService(ILogger<MqttHostedService> logger)
        {
            _logger = logger;

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.ConnectedAsync += OnConnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
            _mqttClient.DisconnectedAsync += OndiSconnectedAsync;
        }


        private async Task OnConnectedAsync(MqttClientConnectedEventArgs args)
        {
            _logger.LogInformation("MQTT connected");

            await _mqttClient.SubscribeAsync("arduino/coucou");

            _logger.LogInformation("subscribed");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .WithClientId("aspnet-client")
                .Build();

            await _mqttClient.ConnectAsync(options, stoppingToken);
        }

        private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic;
            var message = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            _logger.LogInformation($"Message received : {topic} => {message}");

        }

        private Task OndiSconnectedAsync(MqttClientDisconnectedEventArgs args)
        {
            _logger.LogWarning("MQTT disconnected");
            return Task.CompletedTask;
        }
        public async Task ReceiveMessage()
        {
            var mqttFactory = new MqttClientFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("localhost", 1883).Build();

                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine("Received application message");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

                    return Task.CompletedTask;
                };

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().WithTopicTemplate(topicTemplate).Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                Console.WriteLine("MQTT client subscribed to topic");

                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }

        public async Task PublishMessageAsync(string payload)
        {
            var mqttfactory = new MqttClientFactory();

            using (var mqttClient = mqttfactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("localhost", 1883).Build();
                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var applicationMessage = new MqttApplicationMessageBuilder().WithTopicTemplate(topicTemplate).WithPayload(payload).Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                Console.WriteLine("Mqtt apllication message published");
            }
        }
    }
}
