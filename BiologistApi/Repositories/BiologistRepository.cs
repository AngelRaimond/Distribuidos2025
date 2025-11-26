using BiologistApi.Infrastructure.Documents;
using BiologistApi.Models;
using BiologistApi.Mappers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BiologistApi.Infrastructure;

namespace BiologistApi.Repositories;

public class BiologistRepository : IBiologistRepository
{
    private readonly IMongoCollection<BiologistDocument> _biologistsCollection;
public BiologistRepository(IMongoDatabase database, IOptions<MongoDBSettings> settings)
    {
        _biologistsCollection = database.GetCollection<BiologistDocument>(settings.Value.BiologistsCollectionName);

        if (!_biologistsCollection.AsQueryable().Any())
        {
            var biologist = new BiologistDocument
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Name = "test",
                Age = 20,
                BirthDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Areas = new List<AreaDocument>
                {
                    new AreaDocument
                    {
                        Area = "test",
                        SubArea = SubAreaMongo.Morfology
                    }
                }
            };
            _biologistsCollection.InsertOne(biologist);
        }
}


    public async Task<Biologist?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            return null;
            
        var biologist = await _biologistsCollection.Find(t => t.Id == id).FirstOrDefaultAsync(cancellationToken);
        return biologist?.ToModel();
    }

    public async Task<Biologist> CreateAsync(Biologist biologist, CancellationToken cancellationToken)
    {
        biologist.CreatedAt = DateTime.UtcNow;
        var biologistToCreate = biologist.ToDocument();
        await _biologistsCollection.InsertOneAsync(biologistToCreate, null, cancellationToken);
        return biologistToCreate.ToModel();
    }
    
    public async Task<IEnumerable<Biologist>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var biologists = await _biologistsCollection
            .Find(t => t.Name.Contains(name))
            .ToListAsync(cancellationToken);
        return biologists.Select(t => t.ToModel());
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            return false;

        var result = await _biologistsCollection.DeleteOneAsync(t => t.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdateAsync(Biologist biologist, CancellationToken cancellationToken)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(biologist.Id, out _))
            return false;

        var biologistDocument = biologist.ToDocument();
        var result = await _biologistsCollection.ReplaceOneAsync(
            t => t.Id == biologist.Id,
            biologistDocument,
            cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }
}