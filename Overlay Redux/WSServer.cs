using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WatsonWebserver;
using WatsonWebserver.Core;
using WatsonWebserver.Core.WebSockets;

namespace Overlay_Redux
{
    public class WSServer(string host = "localhost", int port = 7777, Action<string, List<string>>? respawnCallback = null)
    {
        public event Action<Dictionary<string, int>>? MedsUpdated;
        public event Action<Dictionary<string, int>>? NadesUpdated;
        public event Action<Dictionary<string, (int Count, string Category)>>? InventoryUpdated;
        public event Action<string>? StatusUpdated;
        public event Action? MatchSetup;
        public event Action? MatchEnded;
        public event Action<JsonElement>? PlayersGot;

        public string? NucleusHash { get; set; }

        private Webserver? _server;
        private WebSocketSession? _currentSession;
        private string? _activePlayer;
        private readonly ConcurrentDictionary<string, Dictionary<string, int>> _allMeds = new();
        private readonly ConcurrentDictionary<string, Dictionary<string, int>> _allNades = new();
        private readonly ConcurrentDictionary<string, Dictionary<string, (int Count, string Category)>> _allItems = new();

        private static readonly Dictionary<string, string> MedsTranslator = new()
        {
            { "Syringe",                    "syringes" },
            { "Med Kit (Level 2)",          "medkits" },
            { "Phoenix Kit (Level 3)",      "phoenixKits" },
            { "Shield Cell",                "shieldCells" },
            { "Shield Battery (Level 2)",   "shieldBatteries" },
            { "Ultimate Accelerant (Level 3)", "ultimateAccelerants" }
        };

        private static readonly Dictionary<string, string> NadesTranslator = new()
        {
            {"mp_weapon_grenade_emp",      "arc"},
            {"mp_weapon_thermite_grenade", "thermite"},
            {"mp_weapon_frag_grenade",     "frag"},
        };

        private static readonly Dictionary<string, (string Key, string Category)> ItemTranslator = new()
        {
            { "Syringe",                        ("syringes",            "meds") },
            { "Med Kit (Level 2)",              ("medkits",             "meds") },
            { "Phoenix Kit (Level 3)",          ("phoenixKits",         "meds") },
            { "Shield Cell",                    ("shieldCells",         "meds") },
            { "Shield Battery (Level 2)",       ("shieldBatteries",     "meds") },
            { "Ultimate Accelerant (Level 3)",  ("ultimateAccelerants", "meds") },
            { "Frag Grenade",                   ("frags",               "nades") },
            { "Thermite Grenade",               ("thermites",           "nades") },
            { "Arc Star",                       ("arcStars",            "nades") },
        };

        public void Start()
        {
            WebserverSettings settings = new(host, port);
            settings.WebSockets.Enable = true;

            _server = new Webserver(settings, DefaultRoute);

            _server.WebSocket("/", async (ctx, session) =>
            {
                Debug.WriteLine($"Client connected: {session.RemoteIp}");
                _currentSession = session;

                await foreach (var message in session.ReadMessagesAsync(ctx.Token))
                {
                    if (message.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                    {
                        try
                        {
                            await HandleMessage(message.Text);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error handling message: {ex.Message}");
                        }
                    }
                }

                StatusUpdated?.Invoke("Disconnected. Attempting to reconnect.");
                _currentSession = null;
                Debug.WriteLine($"Client disconnected: {session.RemoteIp}");
            });

            _server.Start();
            StatusUpdated?.Invoke("Waiting to connect...");
            Debug.WriteLine($"Serving on port {port}...");
        }

        public void Stop()
        {
            _server?.Stop();
            _server?.Dispose();
        }

        private Task HandleMessage(string rawMessage)
        {
            var incoming = JsonSerializer.Deserialize<JsonElement>(rawMessage);

            if (incoming.TryGetProperty("playerToken", out var tokenProp))
            {
                if (!incoming.TryGetProperty("players", out var playerProp)) return Task.CompletedTask;
                PlayersGot?.Invoke(playerProp);
                return Task.CompletedTask;
            }


            if (!incoming.TryGetProperty("category", out var categoryProp))
                return Task.CompletedTask;


            string category = categoryProp.GetString()!;

            switch (category)
            {
                case "init":
                    Debug.WriteLine("Connected!");
                    StatusUpdated?.Invoke("Connected!");
                    break;

                case "playerConnected":
                    string nucleusHash = incoming.GetProperty("player").GetProperty("nucleusHash").GetString()!;
                    DefaultPlayerItems(nucleusHash);
                    break;

                case "matchSetup":
                    MatchSetup?.Invoke();
                    break;

                case "gameStateChanged":
                    if (incoming.GetProperty("state").GetString() == "Resolution")
                    {
                        _allMeds.Clear();
                        _allNades.Clear();
                        _allItems.Clear();
                        _activePlayer = null;
                        MatchEnded?.Invoke();
                    }
                    break;

                case "matchStateEnd":
                    // This event is specifically if the game reaches the "game over"/"champion" screen
                    break;

                case "observerSwitched":
                    if (!string.IsNullOrEmpty(NucleusHash) && incoming.GetProperty("observer").GetProperty("nucleusHash").GetString() != NucleusHash)
                        break;
                    _activePlayer = incoming.GetProperty("target").GetProperty("nucleusHash").GetString()!;
                    FireInventoryUpdated();
                    break;

                case "playerRespawnTeam":
                    string team = incoming.GetProperty("player").GetProperty("teamName").GetString()!;
                    var players = new List<string>();
                    foreach (var player in incoming.GetProperty("respawnedTeammates").EnumerateArray())
                        players.Add(player.GetProperty("name").GetString()!);
                    respawnCallback?.Invoke(team, players);
                    break;

                case "inventoryPickUp":
                    HandleInventoryChange(incoming, delta: 1);
                    break;

                case "inventoryDrop":
                    HandleInventoryChange(incoming, delta: -1);
                    break;

                case "inventoryUse":
                    HandleInventoryChange(incoming, delta: -1);
                    break;
                case "grenadeThrown":
                    HandleLinkedEntityInventoryChange(incoming);
                    break;
                case "respawnFromDeathbox":
                    Debug.WriteLine("------------- Deathbox Respawn -------------");                    
                    Debug.WriteLine(incoming);
                    break;
                case "playerKilled":
                    DefaultPlayerItems(incoming.GetProperty("victim").GetProperty("nucleusHash").GetString()!);
                    break;
            }

            return Task.CompletedTask;
        }

        private void DefaultPlayerItems(string hash)
        {
            _allItems[hash] = new Dictionary<string, (int Count, string Category)>
            {
                { "syringes",            (4, "meds") },
                { "medkits",             (0, "meds") },
                { "phoenixKits",         (0, "meds") },
                { "shieldCells",         (4, "meds") },
                { "shieldBatteries",     (0, "meds") },
                { "ultimateAccelerants", (0, "meds") },
                { "frags",               (0, "nades") },
                { "thermites",           (0, "nades") },
                { "arcStars",            (0, "nades") },
            };
        }

        private void HandleLinkedEntityInventoryChange(JsonElement incoming)
        {
            var modified = new Dictionary<string, object>
            {
                { "item",        incoming.GetProperty("linkedEntity").GetString()! },
                { "quantity",    1 },
                { "player",      incoming.GetProperty("player") }
            };
            var json = JsonSerializer.Serialize(modified);
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            HandleInventoryChange(element, -1);
        }

        private void HandleInventoryChange(JsonElement incoming, int delta)
        {
            string item = incoming.GetProperty("item").GetString()!;

            if (!ItemTranslator.TryGetValue(item, out var entry))
            {
                return;
            }

            string hash = incoming.GetProperty("player").GetProperty("nucleusHash").GetString()!;
            int quantity = incoming.GetProperty("quantity").GetInt32();

            if (_allItems.TryGetValue(hash, out var items))
            {
                (int Count, string Category) = items[entry.Key];
                items[entry.Key] = (Count + delta * quantity, Category);

                if (hash == _activePlayer)
                    FireInventoryUpdated();
            }
        }

        internal async Task<string> SendGetPlayers()
        {
            var message = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "customMatch_GetLobbyPlayers", new { } }
            });
            if (_currentSession == null)
            {
                return ":(";
            }
            await _currentSession!.SendTextAsync(message);
            return ":)";
        }

        public Dictionary<string, int> GetActivePlayerMeds()
        {
            if (_activePlayer != null && _allMeds.TryGetValue(_activePlayer, out var meds))
                return meds;

            return new Dictionary<string, int>
            {
                { "syringes",            0 },
                { "medkits",             0 },
                { "phoenixKits",         0 },
                { "shieldCells",         0 },
                { "shieldBatteries",     0 },
                { "ultimateAccelerants", 0 }
            };
        }

        public Dictionary<string, int> GetActivePlayerNades()
        {
            if (_activePlayer != null && _allNades.TryGetValue(_activePlayer, out var nades))
                return nades;
            return new Dictionary<string, int>
            {
                { "arc", 0 },
                { "thermite", 0 },
                { "frag", 0 }
            };
        }

        private void FireMedsUpdated()
        {
            MedsUpdated?.Invoke(GetActivePlayerMeds());
        }

        private void FireNadesUpdated()
        {
            NadesUpdated?.Invoke(GetActivePlayerNades());
        }
        private void FireInventoryUpdated()
        {
            if (_activePlayer != null && _allItems.TryGetValue(_activePlayer, out var items))
                InventoryUpdated?.Invoke(items.ToDictionary(k => k.Key, v => (v.Value.Count, v.Value.Category)));
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 400;
            await ctx.Response.Send("WebSocket connections only.");
        }
    }
}