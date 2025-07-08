using Microsoft.EntityFrameworkCore;
using TDEV_811.Models;

namespace TDEV_811.Entities.Repositories
{
    public class MqttMessageRepository : IMqttMessageRepository
    {
        private readonly Tdev811Context _context;

        public MqttMessageRepository(Tdev811Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MqttMessage>> GetAllAsync()
        {
            return await _context.MqttMessages.ToListAsync();
        }

        public async Task<MqttMessage> FindByIdAsync(int id)
        {
            return await _context.MqttMessages.FindAsync(id);
        }

        public async Task DeleteAsync(int id)
        {
            var message = await _context.MqttMessages.FindAsync(id);
            if (message != null)
            {
                _context.MqttMessages.Remove(message);
                await _context.SaveChangesAsync();
            }
        }
    }
}
