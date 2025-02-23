namespace AntiDLL.BanModule
{

    using CounterStrikeSharp.API;
    using CounterStrikeSharp.API.Core;
    using CounterStrikeSharp.API.Core.Capabilities;
    using Microsoft.Extensions.Logging;
    using AntiDLL.API;
    using System.Text.Json.Serialization;

    public sealed class Config : BasePluginConfig
    {
        [JsonPropertyName("PunishmentType")] public string PunishmentType { get; set; } = "ban";
        [JsonPropertyName("Banreason")] public string BanReason { get; set; } = "[AntiDLL] Cheats detected";
        [JsonPropertyName("UseSteamID64")] public bool UseSteamID { get; set; } = false;
        [JsonPropertyName("BanCommand")] public string BanCommand { get; set; } = "css_ban";
        [JsonPropertyName("KickCommand")] public string KickCommand { get; set; } = "css_kick";
    }

    public sealed class Plugin : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "[AntiDLL] BanModule";

        public override string ModuleVersion => "1.1";

        public override string ModuleAuthor => "verneri";

        private static PluginCapability<IAntiDLL> AntiDLL { get; } = new PluginCapability<IAntiDLL>("AntiDLL");
        public Config Config { get; set; } = new();
        private HashSet<ulong> Punishedplayers = [];

        private void OnDetection(CCSPlayerController player, string eventName)
        {

            base.Logger.LogInformation("Player {0} detected violating {1}", player.PlayerName, eventName);

            if (!Punishedplayers.Contains(player.SteamID))
            {
                Punishedplayers.Add(player.SteamID);
                Logger.LogInformation($"Added {player.PlayerName} to 'Punishedplayers' list.");


                string identifier = Config.UseSteamID ? player.SteamID.ToString() : $"#{player.UserId}";

                if (string.IsNullOrEmpty(Config.PunishmentType) || (!Config.PunishmentType.Equals("kick", StringComparison.OrdinalIgnoreCase) && !Config.PunishmentType.Equals("ban", StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.LogInformation("Config 'PunishmentType' is invalid. Use 'ban' or 'kick'.");
                    return;
                }

                string command = Config.PunishmentType.Equals("kick", StringComparison.OrdinalIgnoreCase)
                                 ? $"{Config.KickCommand} {identifier} \"{Config.BanReason}\""
                                 : $"{Config.BanCommand} {identifier} 0 \"{Config.BanReason}\"";

                Server.ExecuteCommand(command);
                Logger.LogInformation($"{Config.PunishmentType.ToUpper()}ED {player.PlayerName} for violating {eventName}");

                AddTimer(10.0f, () => Punishedplayers.Remove(player.SteamID));
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
            Config = config;
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
