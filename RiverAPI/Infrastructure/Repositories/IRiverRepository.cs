using RiverAPI.Domain;

namespace RiverAPI.Infrastructure.Repositories
{
    public interface IRiverRepository
    {
        Task<IEnumerable<River>> GetAllAsync(int pageNumber, int pageSize);
        Task<River?> GetByIdAsync(string id);
        Task<IEnumerable<River>> GetByBoundingBoxAsync(double minX, double minY, double maxX, double maxY);
    }
}
