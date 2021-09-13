using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using SharpLink;
//ODg2OTM2OTE5MDI2NTY5MjE2.YT82Xw.DgI3LeyaC3fpUbC8aJXHAl18ZNg
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

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            var token = "ODg2OTM2OTE5MDI2NTY5MjE2.YT82Xw.DgI3LeyaC3fpUbC8aJXHAl18ZNg";

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();


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
