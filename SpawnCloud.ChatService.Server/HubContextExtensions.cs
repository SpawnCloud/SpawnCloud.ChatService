using System.Buffers;
using Microsoft.AspNetCore.SignalR.Protocol;
using Orleans.Concurrency;
using SignalR.Orleans.Core;

namespace SpawnCloud.ChatService.Server;

internal static class HubContextExtensions
{
    public static Task SendToUsers<THub>(this HubContext<THub> hubContext, IReadOnlyCollection<string> users, string methodName, object?[] args) where THub : Microsoft.AspNetCore.SignalR.Hub
    {
        var message = new InvocationMessage(methodName, args).AsImmutable();
        return SendToUsers(hubContext, users, message);
    }

    public static Task SendToUsers<THub>(this HubContext<THub> hubContext, IReadOnlyCollection<string> users, Immutable<InvocationMessage> message) where THub : Microsoft.AspNetCore.SignalR.Hub
    {
        var tasks = ArrayPool<Task>.Shared.Rent(users.Count);
        try
        {
            for (var i = 0; i < users.Count; i++)
            {
                var hubUserGrain = hubContext.User(users.ElementAt(i));
                tasks[i++] = hubUserGrain.Send(message);
            }

            return Task.WhenAll(tasks);
        }
        finally
        {
            ArrayPool<Task>.Shared.Return(tasks);
        }
    }
}