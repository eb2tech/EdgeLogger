using System.Reflection;
using EdgeLogger.ApiService;
using EdgeLogger.ApiService.Services;
using NATS.Client.Core;
using NATS.Net;
using Serilog;

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             .Enrich.FromLogContext()
             .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
             .WriteTo.Console()
             .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddLogging(lb => lb.AddSerilog(dispose: true));

// Add services
builder.Services.AddSingleton<NatsClient>(_ => new NatsClient(new NatsOpts
                                                              {
                                                                  Url = builder.Configuration["Nats:Server"]!,
                                                                  Name = Assembly.GetExecutingAssembly().GetName().Name!,
                                                                  AuthOpts = NatsAuthOpts.Default with
                                                                             {
                                                                                 Username = builder.Configuration["Nats:Username"]!,
                                                                                 Password = builder.Configuration["Nats:Password"]!
                                                                             }
                                                              }));
builder.Services.AddHostedService<AuraLogMessageHandler>();
builder.Services.AddHostedService<NetworkStateMonitorService>();
builder.Services.AddSingleton<INetworkStatus>(sp => sp.GetRequiredService<NetworkStateMonitorService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapApiEndpoints();

app.Run();
