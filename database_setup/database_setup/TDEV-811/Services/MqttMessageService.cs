using TDEV_811.DTO;
using TDEV_811.Entities.Repositories;

namespace TDEV_811.Services
{
    public class MqttMessageService : IMqttMessageService
    {
        private readonly IMqttMessageRepository _messageRepository;

        public MqttMessageService(IMqttMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<IEnumerable<MqttMessageDTO>> GetAllMessageAsync()
        {
            var messages = await _messageRepository.GetAllAsync();
            return messages.Select(x => new MqttMessageDTO
            {
                Id = x.Id,
                Payload = x.Payload,
                Topic = x.Topic,
            });
        }

        public async Task<MqttMessageDTO> GetMessageAsync(int id)
        {
            var message = await _messageRepository.FindByIdAsync(id);
            if (message == null)
                throw new KeyNotFoundException("Message not found");

            return new MqttMessageDTO
            {
                Id = message.Id,
                Payload = message.Payload,
                Topic = message.Topic,
            };
        }

        public async Task DeleteMessageAsync(int id)
        {
            var message = await _messageRepository.FindByIdAsync(id);

            if (message == null)
                throw new KeyNotFoundException("Message not found");

            await _messageRepository.DeleteAsync(id);
        }
    }
}
