﻿namespace AntiDLL
{
    using CounterStrikeSharp.API.Core;

    public sealed class PluginConfig : BasePluginConfig
    {
        public HashSet<string> Blacklist { get; set; } = new HashSet<string>();
    }
}
