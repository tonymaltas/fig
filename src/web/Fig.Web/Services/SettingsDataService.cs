using System.Web;
using Fig.Contracts.SettingDefinitions;
using Fig.Contracts.Settings;
using Fig.Web.Builders;
using Fig.Web.Converters;
using Fig.Web.Models;

namespace Fig.Web.Services;

public class SettingsDataService : ISettingsDataService
{
    private readonly ISettingGroupBuilder _groupBuilder;
    private readonly IHttpService _httpService;
    private readonly ISettingsDefinitionConverter _settingsDefinitionConverter;

    public SettingsDataService(IHttpService httpService,
        ISettingsDefinitionConverter settingsDefinitionConverter,
        ISettingGroupBuilder groupBuilder)
    {
        _httpService = httpService;
        _settingsDefinitionConverter = settingsDefinitionConverter;
        _groupBuilder = groupBuilder;
    }

    public IList<SettingClientConfigurationModel>? SettingsClients { get; private set; }

    public async Task LoadAllClients()
    {
        var settings = await LoadSettings();
        var clients = _settingsDefinitionConverter.Convert(settings);
        clients.AddRange(_groupBuilder.BuildGroups(clients));
        SettingsClients = clients;
    }

    public async Task DeleteClient(SettingClientConfigurationModel client)
    {
        await _httpService.Delete(GetClientUri(client, string.Empty));
    }

    public async Task<Dictionary<SettingClientConfigurationModel, List<string>>> SaveClient(
        SettingClientConfigurationModel client)
    {
        var changedSettings = client.GetChangedSettings();

        foreach (var (clientWithChanges, changesForClient) in changedSettings)
            await SaveChangedSettings(clientWithChanges, changesForClient.ToList());

        return changedSettings.ToDictionary(
            a => a.Key,
            b => b.Value.Select(x => x.Name).ToList());
    }

    private async Task SaveChangedSettings(SettingClientConfigurationModel client,
        List<SettingDataContract> changedSettings)
    {
        if (changedSettings.Any())
            await _httpService.Put(GetClientUri(client), changedSettings);
    }

    private async Task<List<SettingsClientDefinitionDataContract>> LoadSettings()
    {
        return await _httpService.Get<List<SettingsClientDefinitionDataContract>>("/clients");
    }

    private List<SettingsClientDefinitionDataContract> GenerateFakeData()
    {
        return new List<SettingsClientDefinitionDataContract>
        {
            new()
            {
                Name = "MyService1",
                Settings = new List<SettingDefinitionDataContract>
                {
                    new()
                    {
                        Name = "StringSetting",
                        Description = "This is a string setting",
                        Value = "StringValue",
                        IsSecret = true
                    },
                    new()
                    {
                        Name = "StringSetting2",
                        Description = "This is a string setting 2",
                        Value = "StringValue2",
                        ValidationRegex = @"\d{3}",
                        ValidationExplanation = "Should have 3 digits"
                    },
                    new()
                    {
                        Name = "IntSetting",
                        Description = "This is int setting",
                        Value = 5,
                        ValidationRegex = @"\d{3}",
                        ValidationExplanation = "Should have 3 digits"
                    },
                    new()
                    {
                        Name = "BoolSetting",
                        Description = "This is bool setting",
                        Value = true
                    },
                    new()
                    {
                        Name = "Drop Down Setting",
                        Description = "This is a drop down setting",
                        Value = "Dog",
                        ValidValues = new List<string>
                        {
                            "Dog",
                            "Cat",
                            "Rabbit"
                        }
                    }
                }
            },
            new()
            {
                Name = "MyService2",
                Settings = new List<SettingDefinitionDataContract>
                {
                    new()
                    {
                        Name = "StringSetting3",
                        Description = "This is a string setting 3",
                        Value = "StringValue3"
                    },
                    new()
                    {
                        Name = "StringSetting4",
                        Description = "This is a string setting 4",
                        Value = "StringValue4"
                    }
                }
            }
        };
    }

    private string GetClientUri(SettingClientConfigurationModel client, string postRoute = "/settings")
    {
        var clientName = HttpUtility.UrlEncode(client.Name);
        var uri = $"/clients/{clientName}{postRoute}";

        if (client.Instance != null)
            uri += $"?instance={HttpUtility.UrlEncode(client.Instance)}";

        return uri;
    }
}