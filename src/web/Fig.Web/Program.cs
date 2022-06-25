using Fig.Web;
using Fig.Web.Builders;
using Fig.Web.Converters;
using Fig.Web.Facades;
using Fig.Web.Notifications;
using Fig.Web.Services;
using Fig.Web.Utils;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddScoped<IAccountService, AccountService>()
    //.AddScoped<IAlertService, AlertService>()
    .AddScoped<IHttpService, HttpService>()
    .AddScoped<ILocalStorageService, LocalStorageService>();
var apiAddress = Environment.GetEnvironmentVariable("FIG_API_ADDRESS");
//builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});
builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(apiAddress ?? "https://localhost:7281")});
builder.Services.AddScoped<ISettingsDefinitionConverter, SettingsDefinitionConverter>();
builder.Services.AddScoped<IEventLogConverter, EventLogConverter>();
builder.Services.AddScoped<IClientRunSessionConverter, ClientRunSessionConverter>();
builder.Services.AddScoped<ISettingClientFacade, SettingClientFacade>();
builder.Services.AddScoped<IUsersFacade, UsersFacade>();
builder.Services.AddScoped<IUserConverter, UserConverter>();
builder.Services.AddScoped<IEventsFacade, EventsFacade>();
builder.Services.AddScoped<IDataFacade, DataFacade>();
builder.Services.AddScoped<IClientStatusFacade, ClientStatusFacade>();
builder.Services.AddScoped<IApiStatusFacade, ApiStatusFacade>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<INotificationFactory, NotificationFactory>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<ISettingGroupBuilder, SettingGroupBuilder>();
builder.Services.AddScoped<ISettingHistoryConverter, SettingHistoryConverter>();
builder.Services.AddScoped<ISettingVerificationConverter, SettingVerificationConverter>();
builder.Services.AddScoped<IFigConfigurationConverter, FigConfigurationConverter>();
builder.Services.AddScoped<IConfigurationFacade, ConfigurationFacade>();
builder.Services.AddScoped<ICommonEnumerationFacade, CommonEnumerationFacade>();
builder.Services.AddScoped<ICommonEnumerationConverter, CommonEnumerationConverter>();
builder.Services.AddScoped<IApiStatusConverter, ApiStatusConverter>();
builder.Services.AddScoped<IMarkdownExporter, MarkdownExporter>();

var host = builder.Build();

var accountService = host.Services.GetRequiredService<IAccountService>();
await accountService.Initialize();

await host.RunAsync();