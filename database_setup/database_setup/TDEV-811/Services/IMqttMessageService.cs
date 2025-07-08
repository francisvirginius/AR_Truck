using TDEV_811.DTO;

namespace TDEV_811.Services
{
    public interface IMqttMessageService
    {
        Task<IEnumerable<MqttMessageDTO>> GetAllMessageAsync();
        Task<MqttMessageDTO> GetMessageAsync(int id);
        Task DeleteMessageAsync(int id);
    }
}
