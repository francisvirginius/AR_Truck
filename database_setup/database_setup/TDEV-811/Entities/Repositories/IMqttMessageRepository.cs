using TDEV_811.Models;

namespace TDEV_811.Entities.Repositories
{
    public interface IMqttMessageRepository
    {
        Task<IEnumerable<MqttMessage>> GetAllAsync();
        Task<MqttMessage> FindByIdAsync(int id);
        Task DeleteAsync(int id);
    }
}
