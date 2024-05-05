using Microsoft.EntityFrameworkCore;
using SourceUserService;

const string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=SourceUsers;Integrated Security=True;TrustServerCertificate=True;";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<UserDbContext>(options => options
    .UseSqlServer(connectionString));

builder.Services.AddTransient<IUserRepository, UserRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User service");
    c.RoutePrefix = string.Empty;
});

app.MapGet("/users/list", async (UserDbContext _dbContext) =>
{
    return await _dbContext.Users
    .AsNoTracking()
    .ToListAsync();
});

app.MapGet("/users/top/{top}/skip/{skip}", async (int top, int skip, UserDbContext _dbContext) =>
{
    return await _dbContext.Users
        .OrderBy(x => x.Id)
        .Skip(skip)
        .Take(top)
        .AsNoTracking()
        .ToListAsync();
});

app.MapGet("/users", (UserDbContext _dbContext) =>
{
    return _dbContext.Users
        .OrderBy(x => x.Id)
        .AsNoTracking();
});

app.MapPost("/users/generate/{count}", async (int count, IUserRepository _repository) =>
{
    var users = UsersGenerator.GenerateUsers(count);
    await _repository.BulkInsertUsers(users);
});

app.Run();
