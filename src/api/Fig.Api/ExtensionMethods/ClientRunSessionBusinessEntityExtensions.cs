using Fig.Contracts.Status;
using Fig.Datalayer.BusinessEntities;

namespace Fig.Api.ExtensionMethods;

public static class ClientRunSessionBusinessEntityExtensions
{
    public static void Update(
        this ClientRunSessionBusinessEntity runSession, 
        StatusRequestDataContract statusRequest,
        string? hostname, 
        string? ipAddress)
    {
        runSession.Hostname = hostname;
        runSession.IpAddress = ipAddress;
        runSession.LastSeen = DateTime.UtcNow;
        runSession.LiveReload ??= statusRequest.LiveReload;
        runSession.PollIntervalMs ??= statusRequest.PollIntervalMs;
        runSession.UptimeSeconds = statusRequest.UptimeSeconds;
    }
    
    public static bool IsExpired(this ClientRunSessionBusinessEntity session)
    {
        double gracePeriodMs = 2 * session.PollIntervalMs.Value + 50;
        var expiryTime = session.LastSeen.Value + TimeSpan.FromMilliseconds(gracePeriodMs);
        var result = expiryTime < DateTime.UtcNow;
        return result;
    }
}