using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SharpLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DiscordMusicBot
{
    // Create a module with no prefix
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        #region Rickroll
        [Command("rickroll")]
        [Summary("You know what this does.")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task RickRollAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            AudioHandler.PlaySong(user, "https://www.youtube.com/watch?v=dQw4w9WgXcQ", Context.Channel);
        }
        #endregion

        [Command("play")]
        [Summary("Joins the user's channel and plays song")]
        public async Task PlaySong([Remainder][Summary("Song query to play")] string query)
        {
            SocketGuildUser user = Context.User as SocketGuildUser;


            if (!Program.Instance.SongQueues.ContainsKey(user.Guild.Id))
            {
                Program.Instance.SongQueues.Add(user.Guild.Id, new SongQueue());
            }
            if (!Program.Instance.ActiveChannels.ContainsKey(user.Guild.Id))
            {
                Program.Instance.ActiveChannels.Add(user.Guild.Id, Context.Channel);
            }
            else if (Program.Instance.ActiveChannels[user.Guild.Id] != Context.Channel)
            {
                Program.Instance.ActiveChannels[user.Guild.Id] = Context.Channel;
            }

            if (query.Contains("playlist"))
            {
                var links = await AudioHandler.GetPlaylistVideosAsync(query);
                for (int i = 0; i < links.Length; i++)
                {
                    if (i == links.Length-1)
                    {
                        await AudioHandler.AddSongToQueue(user, links[i], true);
                    }
                    else
                    {
                        await AudioHandler.AddSongToQueue(user, links[i], false);
                    }
                }
            }
            else
            {
                await AudioHandler.AddSongToQueue(user, query, true);
            }


        }

        [Command("psong")]
        [Summary("Plays the previous song in the queue")]
        public async Task PlayPreviousSong()
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            AudioHandler.PlayPreviousSong(Context.Guild.Id, user);
        }

        [Command("nsong")]
        [Summary("Plays the next song in the queue")]
        public async Task PlayNextSong()
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            AudioHandler.PlayNextSong(Context.Guild.Id, user);
        }

        [Command("queue")]
        [Summary("Shows the queue")]
        public async Task ShowQueue()
        {
            SocketGuildUser user = Context.User as SocketGuildUser;

            await Context.Channel.SendMessageAsync(embed: new EmbedBuilder().CreateQueueEmbed(user).Build());

        }

    }

    // Create a module with the 'sample' prefix
    [Group("sample")]
    public class SampleModule : ModuleBase<SocketCommandContext>
    {
        // ~sample square 20 -> 400
        [Command("square")]
        [Summary("Squares a number.")]
        public async Task SquareAsync(
            [Summary("The number to square.")]
        int num)
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }

        // ~sample userinfo --> foxbot#0282
        // ~sample userinfo @Khionu --> Khionu#8708
        // ~sample userinfo Khionu#8708 --> Khionu#8708
        // ~sample userinfo Khionu --> Khionu#8708
        // ~sample userinfo 96642168176807936 --> Khionu#8708
        // ~sample whois 96642168176807936 --> Khionu#8708
        [Command("userinfo")]
        [Summary
        ("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync(
            [Summary("The (optional) user to get info from")]
        SocketUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
        }
    }

}
