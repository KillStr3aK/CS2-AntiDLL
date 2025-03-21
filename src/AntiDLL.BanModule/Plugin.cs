namespace AntiDLL.BanModule
{
    using System.Text.Json.Serialization;
    using Microsoft.Extensions.Logging;

    using CounterStrikeSharp.API;
    using CounterStrikeSharp.API.Core;
    using CounterStrikeSharp.API.Core.Capabilities;

    using AntiDLL.API;

    public sealed class Config : BasePluginConfig
    {
        [JsonPropertyName("PunishmentType")] public string PunishmentType { get; set; } = "ban";
        [JsonPropertyName("PunishmentReason")] public string PunishmentReason { get; set; } = "[AntiDLL] Cheats detected";
        [JsonPropertyName("UseSteamID64")] public bool UseSteamID { get; set; } = false;
        [JsonPropertyName("BanCommand")] public string BanCommand { get; set; } = "css_ban";
        [JsonPropertyName("KickCommand")] public string KickCommand { get; set; } = "css_kick";
    }

    public sealed class Plugin : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "[AntiDLL] BanModule";

        public override string ModuleVersion => "1.2";

        public override string ModuleAuthor => "verneri";

        private static PluginCapability<IAntiDLL> AntiDLL { get; } = new PluginCapability<IAntiDLL>("AntiDLL");

        public Config Config { get; set; } = new();

        private HashSet<ulong> Punishedplayers = [];

        private void OnDetection(CCSPlayerController player, string eventName)
        {
            if (player == null || !player.IsValid) return;

            base.Logger.LogInformation("Player {0} detected violating {1}", player.PlayerName, eventName);

            if (!this.Punishedplayers.Contains(player.SteamID))
            {
                this.Punishedplayers.Add(player.SteamID);
                base.Logger.LogInformation($"Added {player.PlayerName} to 'Punishedplayers' list.");

                string identifier = this.Config.UseSteamID ? player.SteamID.ToString() : $"#{player.UserId}";

                if (string.IsNullOrEmpty(this.Config.PunishmentType) || (!this.Config.PunishmentType.Equals("kick", StringComparison.OrdinalIgnoreCase) && !this.Config.PunishmentType.Equals("ban", StringComparison.OrdinalIgnoreCase)))
                {
                    base.Logger.LogInformation("Config 'PunishmentType' is invalid. Use 'ban' or 'kick'.");
                    return;
                }

                string command = this.Config.PunishmentType.Equals("kick", StringComparison.OrdinalIgnoreCase)
                                 ? $"{this.Config.KickCommand} {identifier} \"{this.Config.PunishmentReason}\""
                                 : $"{this.Config.BanCommand} {identifier} 0 \"{this.Config.PunishmentReason}\"";

                Server.ExecuteCommand(command);
                base.Logger.LogInformation($"{this.Config.PunishmentType.ToUpper()}ED {player.PlayerName} for violating {eventName}");

                AddTimer(10.0f, () => this.Punishedplayers.Remove(player.SteamID));
            }
        }

        public override void OnAllPluginsLoaded(bool hotReload)
        {
            IAntiDLL? antidll = AntiDLL.Get();

            if (antidll != null)
            {
                antidll.OnDetection += this.OnDetection;
            }
        }
        public void OnConfigParsed(Config config)
        {
            this.Config = config;
        }

        public override void Unload(bool hotReload)
        {
            IAntiDLL? antidll = AntiDLL.Get();

            if (antidll != null)
            {
                antidll.OnDetection -= this.OnDetection;
            }
        }
    }
}
