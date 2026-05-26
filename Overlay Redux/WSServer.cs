using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WatsonWebserver;
using WatsonWebserver.Core;

namespace Overlay_Redux
{
    public class WSServer
    {
        private readonly string _host;
        private readonly int _port;
        private readonly Action<string, List<string>> _respawnCallback;
        public event Action<Dictionary<string, int>> MedsUpdated;
        public event Action<string> StatusUpdated;

        private Webserver _server;
        private string _activePlayer;
        private readonly ConcurrentDictionary<string, Dictionary<string, int>> _allMeds = new();

        private static readonly Dictionary<string, string> Translator = new()
        {
            { "Syringe",                    "syringes" },
            { "Med Kit (Level 2)",          "medkits" },
            { "Phoenix Kit (Level 3)",      "phoenixKits" },
            { "Shield Cell",                "shieldCells" },
            { "Shield Battery (Level 2)",   "shieldBatteries" },
            { "Ultimate Accelerant (Level 3)", "ultimateAccelerants" }
        };

        public WSServer(string host = "localhost", int port = 7777, Action<string, List<string>> respawnCallback = null)
        {
            _host = host;
            _port = port;
            _respawnCallback = respawnCallback;
        }

        public void Start()
        {
            WebserverSettings settings = new(_host, _port);
            settings.WebSockets.Enable = true;

            _server = new Webserver(settings, DefaultRoute);

            _server.WebSocket("/", async (ctx, session) =>
            {
                Debug.WriteLine($"Client connected: {session.RemoteIp}");

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
                Debug.WriteLine($"Client disconnected: {session.RemoteIp}");
            });

            _server.Start();
            StatusUpdated?.Invoke("Waiting to connect...");
            Debug.WriteLine($"Serving on port {_port}...");
        }

        public void Stop()
        {
            _server?.Stop();
            _server?.Dispose();
        }

        private Task HandleMessage(string rawMessage)
        {
            var incoming = JsonSerializer.Deserialize<JsonElement>(rawMessage);

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
                    _allMeds[nucleusHash!] = new Dictionary<string, int>
                    {
                        { "syringes",           4 },
                        { "medkits",            0 },
                        { "phoenixKits",        0 },
                        { "shieldCells",        4 },
                        { "shieldBatteries",    0 },
                        { "ultimateAccelerants", 0 }
                    };
                    break;

                case "matchStateEnd":
                    _allMeds.Clear();
                    _activePlayer = null;
                    break;

                case "observerSwitched":
                    _activePlayer = incoming.GetProperty("target").GetProperty("nucleusHash").GetString()!;
                    FireMedsUpdated();
                    break;

                case "playerRespawnTeam":
                    string team = incoming.GetProperty("player").GetProperty("teamName").GetString()!;
                    var players = new List<string>();
                    foreach (var player in incoming.GetProperty("respawnedTeammates").EnumerateArray())
                        players.Add(player.GetProperty("name").GetString()!);
                    _respawnCallback?.Invoke(team, players);
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
            }

            return Task.CompletedTask;
        }

        private void HandleInventoryChange(JsonElement incoming, int delta)
        {
            string item = incoming.GetProperty("item").GetString()!;

            if (!Translator.TryGetValue(item, out string medType))
                return;

            string hash = incoming.GetProperty("player").GetProperty("nucleusHash").GetString()!;
            int quantity = incoming.GetProperty("quantity").GetInt32();

            if (_allMeds.TryGetValue(hash, out var meds))
            {
                meds[medType] += delta * quantity;
                if (hash == _activePlayer) FireMedsUpdated();
            }
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

        private void FireMedsUpdated()
        {
            MedsUpdated?.Invoke(GetActivePlayerMeds());
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 400;
            await ctx.Response.Send("WebSocket connections only.");
        }
    }
}