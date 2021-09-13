using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using SharpLink;

namespace DiscordMusicBot
{
    class Program
    {
        public static void Main(string[] args)
         => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandHandler _commandHandler;

        public Dictionary<ulong, SongQueue> SongQueues = new Dictionary<ulong, SongQueue>();
        public Dictionary<ulong, IMessageChannel> ActiveChannels = new Dictionary<ulong, IMessageChannel>();

        public LavalinkManager LavalinkManager;
        public static Program Instance { get; set; }
        public async Task MainAsync()
        {
            Instance = this;
            _client = new DiscordSocketClient();
            //CommandService commandService = new CommandService();
            _commandHandler = new CommandHandler(_client, new CommandService());
            await _commandHandler.InstallCommandsAsync();
            _client.Log += Log;

            LavalinkManager = new LavalinkManager(_client, new LavalinkManagerConfig
            {
                RESTHost = "localhost",
                RESTPort = 2333,
                WebSocketHost = "localhost",
                WebSocketPort = 2333,
                Authorization = "youshallnotpass",
                TotalShards = 1
            });

            LavalinkManager.TrackEnd += AudioHandler.OnTrackEnd;
            _client.Ready += async () =>
            {
                await LavalinkManager.StartAsync();
                //var temp = _client.GetUser(_client.CurrentUser.Id);
                //SocketGuildUser user = temp as SocketGuildUser;

                //if (_client.CurrentUser.)
                {



                }
            };

            //_lavaConfig = new LavaConfig();
            //_lavaNode = new LavaNode(_client, _lavaConfig);

             var token = System.IO.File.ReadAllText("token.txt");



            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
