using Microsoft.EntityFrameworkCore;
using TargetService;

const string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=TargetUsers;Integrated Security=True;TrustServerCertificate=True;";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<UserDbContext>(options => options
    .UseSqlServer(connectionString));

builder.Services.AddTransient<IUserRepository, UserRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Target service");
    c.RoutePrefix = string.Empty;
});

app.MapPost("/init/list", async (IUserRepository _repository, IHttpClientFactory httpClientFactory) =>
{
    using var httpClient = httpClientFactory.CreateClient();
    httpClient.BaseAddress = new("http://sourceUserService");

    var users = await httpClient.GetFromJsonAsync<List<User>>("users/list") ?? throw new InvalidOperationException();

    await _repository.BulkInsertUsers(users);
});

app.MapPost("/init/batches", async (IUserRepository _repository, IHttpClientFactory httpClientFactory) =>
{
    using var httpClient = httpClientFactory.CreateClient();
    httpClient.BaseAddress = new("http://sourceUserService");

    var total = 1_000_000;
    var batchSize = 1000;
    var skip = 0;
    while (skip < total)
    {
        var users = await httpClient.GetFromJsonAsync<List<User>>($"users/top/{batchSize}/skip/{skip}") ?? throw new InvalidOperationException();
        await _repository.BulkInsertUsers(users);
        skip += batchSize;
    }
});

app.MapPost("/init", async (IUserRepository _repository, IHttpClientFactory httpClientFactory) =>
{
    using var httpClient = httpClientFactory.CreateClient();
    httpClient.BaseAddress = new("http://sourceUserService");

    IAsyncEnumerable<User> users = httpClient.GetFromJsonAsAsyncEnumerable<User>("users");

    await _repository.BulkInsertUsers(users.ToBlockingEnumerable());
});

app.MapPost("/sync", async (IUserRepository _repository, IHttpClientFactory httpClientFactory) =>
{
    using var httpClient = httpClientFactory.CreateClient();
    httpClient.BaseAddress = new("http://sourceUserService");

    var sourceUsers = httpClient.GetFromJsonAsAsyncEnumerable<User>("users");
    var targetUsers = _repository.GetAllUsers();

    var diff = UsersDiff.Execute(sourceUsers.ToBlockingEnumerable(), targetUsers);

    await _repository.BulkSyncUsers(diff);
});

app.Run();
