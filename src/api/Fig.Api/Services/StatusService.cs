using Fig.Api.Converters;
using Fig.Api.Datalayer.Repositories;
using Fig.Api.ExtensionMethods;
using Fig.Api.Utils;
using Fig.Contracts.Status;
using Fig.Datalayer.BusinessEntities;

namespace Fig.Api.Services;

public class StatusService : IStatusService
{
    private readonly IClientStatusConverter _clientStatusConverter;
    private readonly IClientStatusRepository _clientStatusRepository;
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IMemoryLeakAnalyzer _memoryLeakAnalyzer;
    private readonly IEventLogFactory _eventLogFactory;
    private readonly IEventLogRepository _eventLogRepository;
    private readonly ILogger<StatusService> _logger;
    private string? _requesterHostname;
    private string? _requestIpAddress;

    public StatusService(
        IClientStatusRepository clientStatusRepository,
        IEventLogRepository eventLogRepository,
        IEventLogFactory eventLogFactory,
        IClientStatusConverter clientStatusConverter,
        IConfigurationRepository configurationRepository,
        IMemoryLeakAnalyzer memoryLeakAnalyzer,
        ILogger<StatusService> logger)
    {
        _clientStatusRepository = clientStatusRepository;
        _eventLogRepository = eventLogRepository;
        _eventLogFactory = eventLogFactory;
        _clientStatusConverter = clientStatusConverter;
        _configurationRepository = configurationRepository;
        _memoryLeakAnalyzer = memoryLeakAnalyzer;
        _logger = logger;
    }

    public StatusResponseDataContract SyncStatus(
        string clientName,
        string? instance,
        string clientSecret,
        StatusRequestDataContract statusRequest)
    {
        var client = _clientStatusRepository.GetClient(clientName, instance);

        if (client is null && !string.IsNullOrEmpty(instance))
            client = _clientStatusRepository.GetClient(clientName);
        
        if (client is null)
            throw new KeyNotFoundException();

        if (!BCrypt.Net.BCrypt.EnhancedVerify(clientSecret, client.ClientSecret))
            throw new UnauthorizedAccessException();

        var session = client.RunSessions.FirstOrDefault(a => a.RunSessionId == statusRequest.RunSessionId);
        if (session is not null)
        {
            if (session.HasConfigurationError != statusRequest.HasConfigurationError)
                LogConfigurationErrorStatus(statusRequest, client);
            
            session.Update(statusRequest, _requesterHostname, _requestIpAddress);
        }
        else
        {
            session = new ClientRunSessionBusinessEntity
            {
                RunSessionId = statusRequest.RunSessionId
            };
            session.Update(statusRequest, _requesterHostname, _requestIpAddress);
            client.RunSessions.Add(session);
            _eventLogRepository.Add(_eventLogFactory.NewSession(session, client));
            if (statusRequest.HasConfigurationError)
                LogConfigurationErrorStatus(statusRequest, client);
        }

        RemoveExpiredSessions(client);

        var memoryAnalysis = _memoryLeakAnalyzer.AnalyzeMemoryUsage(session);
        if (memoryAnalysis is not null)
            session.MemoryAnalysis = memoryAnalysis;
        
        _clientStatusRepository.UpdateClientStatus(client);
        var configuration = _configurationRepository.GetConfiguration();

        return new StatusResponseDataContract
        {
            SettingUpdateAvailable = client.LastSettingValueUpdate > statusRequest.LastSettingUpdate,
            PollIntervalMs = session.PollIntervalMs,
            LiveReload = session.LiveReload ?? true,
            AllowOfflineSettings = configuration.AllowOfflineSettings,
            RestartRequested = session.RestartRequested
        };
    }

    public ClientConfigurationDataContract UpdateConfiguration(string clientName, string? instance,
        ClientConfigurationDataContract updatedConfiguration)
    {
        var client = _clientStatusRepository.GetClient(clientName, instance);
        if (client == null)
            throw new KeyNotFoundException();

        var session = client.RunSessions.FirstOrDefault(a => a.RunSessionId == updatedConfiguration.RunSessionId);
        if (session is null)
            throw new KeyNotFoundException();

        session.LiveReload = updatedConfiguration.LiveReload;
        if (updatedConfiguration.PollIntervalMs is not null)
            session.PollIntervalMs = updatedConfiguration.PollIntervalMs.Value;
        session.RestartRequested = updatedConfiguration.RestartRequested;

        _clientStatusRepository.UpdateClientStatus(client);
        return updatedConfiguration;
    }

    public List<ClientStatusDataContract> GetAll()
    {
        var clients = _clientStatusRepository.GetAllClients();
        return clients.Select(a => _clientStatusConverter.Convert(a))
            .Where(a => a.RunSessions.Any())
            .ToList();
    }

    public void SetRequesterDetails(string? ipAddress, string? hostname)
    {
        _requestIpAddress = ipAddress;
        _requesterHostname = hostname;
    }

    private void RemoveExpiredSessions(ClientStatusBusinessEntity client)
    {
        foreach (var session in client.RunSessions.ToList())
        {
            _logger.LogInformation(
                $"{session.Id}. Last seen:{session.LastSeen}. Poll interval: {session.PollIntervalMs}");
            if (session.IsExpired())
            {
                client.RunSessions.Remove(session);
                _eventLogRepository.Add(_eventLogFactory.ExpiredSession(session, client));
            }
        }
    }
    
    private void LogConfigurationErrorStatus(StatusRequestDataContract statusRequest,
        ClientStatusBusinessEntity client)
    {
        _eventLogRepository.Add(_eventLogFactory.ConfigurationErrorStatusChanged(client, statusRequest));
        
        foreach (var configurationError in statusRequest.ConfigurationErrors)
            _eventLogRepository.Add(_eventLogFactory.ConfigurationError(client, configurationError));
            
    }
}