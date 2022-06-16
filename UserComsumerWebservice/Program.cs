using Commun;
using UserComsumerWebservice.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHostedService<MainLoopService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddHttpClient("SyncClient",
        c => { c.DefaultRequestHeaders.Add(Constants.RBS_TOKEN_NAME, Constants.RBS_TOKEN_VALUE); })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AllowAutoRedirect = true,
        UseCookies = false
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
