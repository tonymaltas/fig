using Fig.Api;
using Fig.Api.Converters;
using Fig.Api.Datalayer;
using Fig.Api.Datalayer.Repositories;
using Fig.Api.Services;
using Fig.Api.SettingVerification;
using Fig.Api.SettingVerification.Dynamic;
using Fig.Api.SettingVerification.Plugin;
using Fig.Api.Validators;
using Fig.Contracts.SettingVerification;
using Fig.Datalayer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IClientSecretValidator, ClientSecretValidator>();
builder.Services.AddSingleton<ISettingConverter, SettingConverter>();
builder.Services.AddSingleton<IFigSessionFactory, FigSessionFactory>();
builder.Services.AddSingleton<IEventLogFactory, EventLogFactory>();
builder.Services.AddSingleton<ISettingDynamicVerification, SettingDynamicVerification>();
builder.Services.AddSingleton<ISettingVerificationConverter, SettingVerificationConverter>();
builder.Services.AddSingleton<ISettingDefinitionConverter, SettingDefinitionConverter>();

builder.Services.AddSingleton<ISettingDynamicVerification, SettingDynamicVerification>();
builder.Services.AddSingleton<ISettingPluginVerification, SettingPluginVerification>();
builder.Services.AddSingleton<ISettingVerifier, SettingVerifier>();

builder.Services.AddSingleton<ISettingClientRepository, SettingClientClientRepository>();
builder.Services.AddSingleton<IEventLogRepository, EventLogRepository>();
builder.Services.AddSingleton<ISettingHistoryRepository, SettingHistoryRepository>();
builder.Services.AddSingleton<ISettingsService, SettingsService>();

// Newtonsoft.Json is required because the client is .net standard and must use that serializer.
builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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