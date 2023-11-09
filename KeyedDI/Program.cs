var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedSingleton<IStorageProvider>("local", new LocalStorageProvider());
builder.Services.AddKeyedSingleton<IStorageProvider>("azure", new AzureStorageProvider());

var app = builder.Build();

app.MapGet("/local/{name}", ([FromKeyedServices("local")] IStorageProvider provider, string name) => provider.GetDocument(name));
app.MapGet("/azure/{name}", ([FromKeyedServices("azure")] IStorageProvider provider, string name) => provider.GetDocument(name));

app.Run();

public interface IStorageProvider
{
    Task<string> GetDocument(string name);
}

public class LocalStorageProvider : IStorageProvider
{
    public Task<string> GetDocument(string name)
    {
        return Task.FromResult($"Resolving {name} from local storage.");
    }
}

public class AzureStorageProvider : IStorageProvider
{
        public Task<string> GetDocument(string name)
    {
        return Task.FromResult($"Resolving {name} from Azure storage.");
    }
}