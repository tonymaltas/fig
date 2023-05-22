using Fig.Common.NetStandard.WebHook;
using Fig.Datalayer.BusinessEntities;

namespace Fig.Api.Converters;

public class WebHookClientConverter : IWebHookClientConverter
{
    public WebHookClientDataContract Convert(WebHookClientBusinessEntity client)
    {
        return new WebHookClientDataContract(client.Id, client.Name, new Uri(client.BaseUri), client.HashedSecret);
    }

    public WebHookClientBusinessEntity Convert(WebHookClientDataContract client)
    {
        var businessEntity = new WebHookClientBusinessEntity
        {
            Name = client.Name,
            BaseUri = client.BaseUri.ToString(),
            HashedSecret = client.HashedSecret
        };

        if (client.Id is not null)
            businessEntity.Id = client.Id.Value;

        return businessEntity;
    }
}