﻿using Orleans;
using SpawnCloud.ChatService.Shared.Contracts;

namespace SpawnCloud.ChatService.Shared.Grains;

public interface IChatChannelGrain : IGrainWithGuidKey
{
    Task<ChannelDescription> GetDescription();
}