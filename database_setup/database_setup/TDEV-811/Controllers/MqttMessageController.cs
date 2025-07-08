using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.Extensions.TopicTemplate;
using TDEV_811.Services;

namespace TDEV_811.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MqttMessageController : ControllerBase
    {
        private readonly IMqttMessageService _messageService;
        readonly MqttTopicTemplate topicTemplate = new("arduino/coucou");


        public MqttMessageController (IMqttMessageService messageService)
        {
            _messageService = messageService;
        }

    }
}
