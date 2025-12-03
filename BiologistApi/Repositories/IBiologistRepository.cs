using BiologistApi.Models;

namespace BiologistApi.Repositories;

public interface IBiologistRepository
{
    Task<Biologist?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<Biologist> CreateAsync(Biologist biologist, CancellationToken cancellationToken);
    Task<IEnumerable<Biologist>> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Biologist biologist, CancellationToken cancellationToken);
}