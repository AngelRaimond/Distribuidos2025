using TrainerApi.Infrastructure.Documents;
using TrainerApi.Models;
using TrainerApi.Mappers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TrainerApi.Infrastructure;

namespace TrainerApi.Repositories;

public class TrainerRepository : ITrainerRepository
{
    private readonly IMongoCollection<TrainerDocument> _trainersCollection;
public TrainerRepository(IMongoDatabase database, IOptions<MongoDBSettings> settings)
    {
        _trainersCollection = database.GetCollection<TrainerDocument>(settings.Value.TrainersCollectionName);

        if (!_trainersCollection.AsQueryable().Any())
        {
            var trainer = new TrainerDocument
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Name = "test",
                Age = 20,
                BirthDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Medals = new List<MedalDocument>
                {
                    new MedalDocument
                    {
                        Region = "test",
                        Type = Infrastructure.Documents.MedalType.Gold
                    }
                }
            };
            _trainersCollection.InsertOne(trainer);
        }
}


    public async Task<Trainer?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            return null;
            
        var trainer = await _trainersCollection.Find(t => t.Id == id).FirstOrDefaultAsync(cancellationToken);
        return trainer?.ToModel();
    }

    public async Task<Trainer> CreateAsync(Trainer trainer, CancellationToken cancellationToken)
    {
        trainer.CreatedAt = DateTime.UtcNow;
        var trainerToCreate = trainer.ToDocument();
        await _trainersCollection.InsertOneAsync(trainerToCreate, null, cancellationToken);
        return trainerToCreate.ToModel();
    }
    
    public async Task<IEnumerable<Trainer>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var trainers = await _trainersCollection
            .Find(t => t.Name.Contains(name))
            .ToListAsync(cancellationToken);
        return trainers.Select(t => t.ToModel());
    }
}