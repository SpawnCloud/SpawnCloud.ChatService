using Microsoft.AspNetCore.SignalR;
using OpenIddict.Abstractions;

namespace SpawnCloud.ChatService.Hub;

internal class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User.GetClaim("sub");
    }
}