using Microsoft.EntityFrameworkCore;
using PlantApi.Infrastructure;
using PlantApi.Models;
using PlantApi.Mappers;

namespace PlantApi.Repositories;

public class PlantRepository : IPlantRepository
{
    private readonly RelationalDbContext _context;

    public PlantRepository(RelationalDbContext context)
    {
        _context = context;

    }
    
    public async Task UpdatePlantAsync(Plant plant, CancellationToken cancellationToken)
    {
        _context.Plants.Update(plant.ToEntity());
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePlantAsync(Plant plant, CancellationToken cancellationToken)
    {
        _context.Plants.Remove(plant.ToEntity());
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Plant>> GetPlantsByNameAsync(string name, CancellationToken cancellationToken)
    {
        var plants = await _context.Plants.AsNoTracking()
        .Where(s => s.Name.Contains(name)).ToListAsync(cancellationToken);
        return plants.ToModel();
    }

    public async Task<Plant?> GetPlantByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var plant = await _context.Plants.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        return plant?.ToModel();
    }

    public async Task<Plant?> GetPlantsByFamilyAsync(string family, CancellationToken cancellationToken)
    {
        var plant = await _context.Plants.AsNoTracking().FirstOrDefaultAsync(s => s.Family.Contains(family), cancellationToken);
        return plant?.ToModel();
    }

    public async Task<Plant> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var plant = await _context.Plants.AsNoTracking().FirstOrDefaultAsync(s => s.Name.Contains(name));
        return plant.ToModel();
    }

    public async Task<Plant> CreateAsync(Plant plant, CancellationToken cancellationToken)
    {
        var plantToCreate = plant.ToEntity();
        plantToCreate.Id = Guid.NewGuid();
        await _context.Plants.AddAsync(plantToCreate, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return plantToCreate.ToModel();
    }
}