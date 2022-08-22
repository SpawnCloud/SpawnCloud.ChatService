using Microsoft.AspNetCore.SignalR;
using OpenIddict.Abstractions;

namespace SpawnCloud.ChatService.Hub;

public class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User.GetClaim("sub");
    }
}