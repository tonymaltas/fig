using System;

namespace Fig.Common.NetStandard.WebHook;

public class WebHookDataContract
{
    public WebHookDataContract(Guid? id,
        WebHookType webHookType,
        string? clientNameRegex,
        string? settingNameRegex,
        int minSessions)
    {
        Id = id;
        WebHookType = webHookType;
        ClientNameRegex = clientNameRegex;
        SettingNameRegex = settingNameRegex;
        MinSessions = minSessions;
    }

    public Guid? Id { get; set; }
    
    public WebHookType WebHookType { get; set; }
    
    public string? ClientNameRegex { get; set; }
    
    public string? SettingNameRegex { get; set; }
    
    public int MinSessions { get; set; }
}