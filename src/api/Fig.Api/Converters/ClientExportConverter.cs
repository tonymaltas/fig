﻿using Fig.Api.Exceptions;
using Fig.Api.ExtensionMethods;
using Fig.Api.Services;
using Fig.Contracts;
using Fig.Contracts.ImportExport;
using Fig.Contracts.SettingDefinitions;
using Fig.Contracts.Settings;
using Fig.Datalayer.BusinessEntities;
using Fig.Datalayer.BusinessEntities.SettingValues;
using Newtonsoft.Json;

namespace Fig.Api.Converters;

public class ClientExportConverter : IClientExportConverter
{
    private readonly IEncryptionService _encryptionService;
    private readonly ISettingConverter _settingConverter;

    public ClientExportConverter(IEncryptionService encryptionService, ISettingConverter settingConverter)
    {
        _encryptionService = encryptionService;
        _settingConverter = settingConverter;
    }

    public SettingClientExportDataContract Convert(SettingClientBusinessEntity client)
    {
        return new SettingClientExportDataContract(client.Name,
            client.Description,
            client.ClientSecret,
            client.Instance,
            client.Settings
                .Select(Convert).ToList(),
            client.Verifications.Select(Convert).ToList());
    }

    public SettingClientValueExportDataContract ConvertValueOnly(SettingClientBusinessEntity client)
    {
        return new SettingClientValueExportDataContract(
            client.Name,
            client.Instance,
            client.Settings
                .Select(ConvertValueOnlySetting).ToList());
    }

    private SettingValueExportDataContract ConvertValueOnlySetting(SettingBusinessEntity setting)
    {
        var value = setting.Value?.GetValue();
        if (setting.IsSecret && value is not null)
        {
            value = _encryptionService.Encrypt(value.ToString());
        }
        
        return new SettingValueExportDataContract(setting.Name, value, setting.IsSecret);
    }

    public SettingClientBusinessEntity Convert(SettingClientExportDataContract client)
    {
        return new SettingClientBusinessEntity
        {
            Name = client.Name,
            Description = client.Description,
            ClientSecret = client.ClientSecret,
            Instance = client.Instance,
            LastRegistration = DateTime.UtcNow,
            Settings = client.Settings.Select(Convert).ToList(),
            Verifications = client.Verifications.Select(Convert).ToList()
        };
    }

    private SettingVerificationBusinessEntity Convert(VerificationExportDataContract verification)
    {
        return new SettingVerificationBusinessEntity
        {
            Name = verification.Name,
            PropertyArguments = verification.PropertyArguments
        };
    }

    private SettingBusinessEntity Convert(SettingExportDataContract setting)
    {
        var dataGridDefinition = setting.DataGridDefinitionJson is null
            ? null
            : JsonConvert.DeserializeObject<DataGridDefinitionDataContract>(setting.DataGridDefinitionJson);
        SettingValueBaseBusinessEntity? value;
        if (setting is { IsEncrypted: true, Value: StringSettingDataContract strValue })
        {
            value = _settingConverter.Convert(GetDecryptedValue(strValue, setting.ValueType, setting.Name));
        }
        else 
        {
            if (dataGridDefinition is not null && dataGridDefinition.Columns.Any(a => a.IsSecret))
            {
                var dataGridValue = setting.Value?.GetValue() as List<Dictionary<string, object?>>;
                foreach (var column in dataGridDefinition.Columns.Where(a => a.IsSecret))
                {
                    foreach (var row in dataGridValue ?? [])
                    {
                        if (row[column.Name] is not null)
                        {
                            row[column.Name] = _encryptionService.Decrypt(row[column.Name]!.ToString());
                        }
                    }
                }
            }
            
            value = _settingConverter.Convert(setting.Value);
        }

        return new SettingBusinessEntity
        {
            Name = setting.Name,
            Description = setting.Description,
            IsSecret = setting.IsSecret,
            ValueType = setting.ValueType,
            Value = value,
            DefaultValue = _settingConverter.Convert(setting.DefaultValue),
            JsonSchema = setting.JsonSchema,
            ValidationRegex = setting.ValidationRegex,
            ValidationExplanation = setting.ValidationExplanation,
            ValidValues = setting.ValidValues,
            Group = setting.Group,
            DisplayOrder = setting.DisplayOrder,
            Advanced = setting.Advanced,
            LookupTableKey = setting.LookupTableKey,
            EditorLineCount = setting.EditorLineCount,
            DataGridDefinitionJson = setting.DataGridDefinitionJson,
            EnablesSettings = setting.EnablesSettings,
            SupportsLiveUpdate = setting.SupportsLiveUpdate,
            LastChanged = setting.LastChanged,
            CategoryColor = setting.CategoryColor,
            CategoryName = setting.CategoryName,
            DisplayScript = setting.DisplayScript
        };
    }

    private SettingExportDataContract Convert(SettingBusinessEntity setting)
    {
        var dataGridDefinition = setting.GetDataGridDefinition();
        var value = _settingConverter.Convert(setting.Value, setting.HasSchema());
        var isEncrypted = false;
        if (setting.IsSecret && value?.GetValue() is not null)
        {
            var encryptedValue = _encryptionService.Encrypt(value.GetValue()!.ToString());
            value = new StringSettingDataContract(encryptedValue);
            isEncrypted = true;
        }
        else if (dataGridDefinition is not null && dataGridDefinition.Columns.Any(a => a.IsSecret))
        {
            var dataGridValue = value?.GetValue() as List<Dictionary<string, object?>>;
            foreach (var column in dataGridDefinition.Columns.Where(a => a.IsSecret))
            {
                foreach (var row in dataGridValue ?? [])
                {
                    if (row[column.Name] is not null)
                    {
                        row[column.Name] = _encryptionService.Encrypt(row[column.Name]!.ToString());
                    }
                }
            }
        }

        return new SettingExportDataContract(
            setting.Name,
            setting.Description,
            setting.IsSecret,
            setting.ValueType,
            value,
            _settingConverter.Convert(setting.DefaultValue, setting.HasSchema(), null), // TODO: Pass in real definition
            isEncrypted,
            setting.JsonSchema,
            setting.ValidationRegex,
            setting.ValidationExplanation,
            setting.ValidValues,
            setting.Group,
            setting.DisplayOrder,
            setting.Advanced,
            setting.LookupTableKey,
            setting.EditorLineCount,
            setting.DataGridDefinitionJson,
            setting.EnablesSettings,
            setting.SupportsLiveUpdate,
            setting.LastChanged,
            setting.CategoryColor,
            setting.CategoryName,
            setting.DisplayScript);
    }

    private VerificationExportDataContract Convert(SettingVerificationBusinessEntity verification)
    {
        return new VerificationExportDataContract(verification.Name, verification.PropertyArguments);
    }

    private SettingValueBaseDataContract? GetDecryptedValue(StringSettingDataContract settingValue, Type type, string settingName)
    {
        try
        {
            var value = _encryptionService.Decrypt(settingValue.Value);
            return value is null ? null : ValueDataContractFactory.CreateContract(value, type);
        }
        catch (Exception)
        {
            throw new InvalidPasswordException($"Unable to decrypt password for setting {settingName}");
        }
    }
}