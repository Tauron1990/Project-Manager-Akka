﻿namespace ServiceHost.Client.Shared.ConfigurationServer.Data
{
    public sealed record GlobalConfig(string ConfigContent, string? Info) : ConfigElement(ConfigContent, Info);
}