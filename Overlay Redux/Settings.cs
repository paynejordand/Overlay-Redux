using System.Text.Json.Serialization;

namespace Overlay_Redux
{
    public class Settings
    {
        // General
        [JsonPropertyName("medsWindowActive")]
        public bool MedsWindowActive { get; set; } = true;

        [JsonPropertyName("respawnWindowActive")]
        public bool RespawnWindowActive { get; set; } = true;

        [JsonPropertyName("nucleusHash")]
        public string? NucleusHash { get; set; } = null;

        // Meds
        [JsonPropertyName("medsBackground")]
        public string MedsBackground { get; set; } = "#A9A9A9";
        // FF0000
        [JsonPropertyName("medsBorderBrush")]
        public string MedsBorderBrush { get; set; } = "#800080";

        [JsonPropertyName("medsBorderThickness")]
        public double MedsBorderThickness { get; set; } = 1.25;

        [JsonPropertyName("medsTextForeground")]
        public string MedsTextForeground { get; set; } = "#FFFFFF";

        // Respawn
        [JsonPropertyName("respawnBackground")]
        public string RespawnBackground { get; set; } = "#90EE90";

        [JsonPropertyName("respawnTextForeground")]
        public string RespawnTextForeground { get; set; } = "#000000";
    }
}