using EventBroadcaster.BackgroundServices;
using EventBroadcaster.Entities;
using EventBroadcaster.Extension;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<InternalTokenAuth>();
builder.Services.AddControllers();
builder.Services.AddDbContext<DataDbContext>();
builder.Services.AddHostedService<MainEventLoop>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
