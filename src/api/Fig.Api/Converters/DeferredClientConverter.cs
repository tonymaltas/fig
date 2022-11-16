using Fig.Contracts.Authentication;
using Fig.Contracts.ImportExport;
using Fig.Datalayer.BusinessEntities;
using Newtonsoft.Json;

namespace Fig.Api.Converters;

public class DeferredClientConverter : IDeferredClientConverter
{
    public DeferredClientImportBusinessEntity Convert(SettingClientValueExportDataContract client, UserDataContract? user)
    {
        return new DeferredClientImportBusinessEntity
        {
            Name = client.Name,
            Instance = client.Instance,
            SettingValuesAsJson = JsonConvert.SerializeObject(client.Settings),
            SettingCount = client.Settings.Count,
            AuthenticatedUser = user?.Username ?? "Unknown"
        };
    }

    public SettingClientValueExportDataContract Convert(DeferredClientImportBusinessEntity client)
    {
        return new SettingClientValueExportDataContract(
            client.Name,
            client.Instance,
            JsonConvert.DeserializeObject<List<SettingValueExportDataContract>>(client.SettingValuesAsJson) ?? new List<SettingValueExportDataContract>());
    }
}