namespace BiologistApi.Infrastructure;

public class MongoDBSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string BiologistsCollectionName { get; set; } = null!;
}
