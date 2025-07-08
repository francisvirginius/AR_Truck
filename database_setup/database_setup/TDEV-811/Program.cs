using TDEV_811.Services;
using TDEV_811.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using TDEV_811;
using dotenv.net;
using MQTTnet;
using TDEV_811.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false));
var envVars = DotEnv.Read();


var dbName = envVars["POSTGRES_DB"];
var dbUser = envVars["POSTGRES_USER"];
var dbPassword = envVars["POSTGRES_PASSWORD"];


builder.Services.AddDbContext<Tdev811Context>(options =>
{
    options.UseNpgsql(
        $"Host=localhost;" +
        $"Database={dbName};" +
        $"Username={dbUser};" +
        $"Password={dbPassword}"
    );
});


builder.Services.AddHostedService<MqttHostedService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<Tdev811Context, Tdev811Context>();

builder.Services.AddConnections();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

    
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();