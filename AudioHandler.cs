using Discord;
using Discord.WebSocket;
using SharpLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Common;

namespace DiscordMusicBot
{
    public static class AudioHandler
    {
        private static LavalinkManager _lavalinkManager { get => Program.Instance.LavalinkManager; }

        public static async Task<LavalinkPlayer> JoinUsersChannel(SocketGuildUser user)
        {
            IVoiceChannel voiceChannel = user.VoiceChannel;
            return _lavalinkManager.GetPlayer(user.Guild.Id) ?? await _lavalinkManager.JoinAsync(voiceChannel);
        }
        public static async Task AddSongToQueue(SocketGuildUser user, string query, bool generateQueueEmbed)
        {
            LoadTracksResponse response;

            if (query.StartsWith("https"))
            {
                response = await _lavalinkManager.GetTracksAsync(query);
            }
            else
            {
                response = await _lavalinkManager.GetTracksAsync($"ytsearch:" + query);
            }
            LavalinkTrack track = response.Tracks.First();

            Program.Instance.SongQueues[user.Guild.Id].Enqueue(track);

            if (generateQueueEmbed) await Program.Instance.ActiveChannels[user.Guild.Id].SendMessageAsync(embed: new EmbedBuilder().CreateQueueEmbed(user).Build());

            if (Program.Instance.SongQueues[user.Guild.Id].Count == 1)
            {
                PlayNextSong(user.Guild.Id, user);
            }
        }
        public static async void PlaySong(SocketGuildUser user, string query, ISocketMessageChannel channel)
        {
            var player = await JoinUsersChannel(user);

            LoadTracksResponse response;

            if (query.StartsWith("https"))
            {
                response = await _lavalinkManager.GetTracksAsync(query);
            }
            else
            {
                response = await _lavalinkManager.GetTracksAsync($"ytsearch:" + query);
            }
            LavalinkTrack track = response.Tracks.First();

            var embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = "Now playing " + track.Title,
                Description = "By " + track.Author + Environment.NewLine + track.Url,
                Color = Color.Orange,
                ImageUrl = "https://img.youtube.com/vi/" + track.Identifier + "/0.jpg"
            };
            embed.WithCurrentTimestamp();
            embed.WithAuthor(user);
            //Your embed needs to be built before it is able to be sent
            await channel.SendMessageAsync(embed: embed.Build());

            await player.PlayAsync(track);
        }
        public static async void PlaySong(SocketGuildUser user, LavalinkTrack track, ISocketMessageChannel channel)
        {
            var player = await JoinUsersChannel(user);
            var embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = "Now playing " + track.Title,
                Description = "By " + track.Author + Environment.NewLine + track.Url,
                Color = Color.Orange,
                ImageUrl = "https://img.youtube.com/vi/" + track.Identifier + "/0.jpg"
            };
            embed.WithCurrentTimestamp();
            embed.WithAuthor(user);
            //Your embed needs to be built before it is able to be sent
            await channel.SendMessageAsync(embed: embed.Build());

            await player.PlayAsync(track);
        }
        public static async void PlaySong(LavalinkPlayer player, LavalinkTrack track)
        {
            await player.PlayAsync(track);

            var embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = "Now playing " + track.Title,
                Description = "By " + track.Author + Environment.NewLine + track.Url,
                Color = Color.Orange,
                ImageUrl = "https://img.youtube.com/vi/" + track.Identifier + "/0.jpg"
            };
            embed.WithCurrentTimestamp();
            //Your embed needs to be built before it is able to be sent
            await Program.Instance.ActiveChannels[player.VoiceChannel.GuildId].SendMessageAsync(embed: embed.Build());
        }

        public static async void LeaveChannel(ulong guildId)
        {
            await _lavalinkManager.LeaveAsync(guildId);
        }

        public static async void Pause(ulong guildId)
        {
            await _lavalinkManager.GetPlayer(guildId).PauseAsync();
        } 
        public static async void Resume(ulong guildId)
        {
            await _lavalinkManager.GetPlayer(guildId).ResumeAsync();
        }

        public static async Task OnTrackEnd(LavalinkPlayer lavalinkPlayer, LavalinkTrack track, string reason)
        {
            Console.WriteLine("Song ended:- " + reason);
            if (reason == "FINISHED")
            {
                PlayNextSong(lavalinkPlayer.VoiceChannel.GuildId);
            }
        }    
        public async static void PlayNextSong(ulong guildId, SocketGuildUser user = null)
        {
            LavalinkTrack trackToPlay = Program.Instance.SongQueues[guildId].NextSong();

            var player = _lavalinkManager.GetPlayer(guildId);
            if (player == null)
            {
                player = await JoinUsersChannel(user);
            }
            PlaySong(_lavalinkManager.GetPlayer(guildId), trackToPlay);
        }
        public async static void PlayPreviousSong(ulong guildId, SocketGuildUser user = null)
        {
            LavalinkTrack trackToPlay = Program.Instance.SongQueues[guildId].PreviousSong();

            var player = _lavalinkManager.GetPlayer(guildId);
            if (player == null)
            {
                player = await JoinUsersChannel(user);
            }
            PlaySong(_lavalinkManager.GetPlayer(guildId), trackToPlay);
        }
        public static EmbedBuilder CreateQueueEmbed(this EmbedBuilder embedBuilder, SocketGuildUser user)
        {
            string queueString = "";

            foreach (var song in Program.Instance.SongQueues[user.Guild.Id].GetAllTracks())
            {
                string title = song.Title.Replace(song.Author, "");
                string author = song.Author.Replace(" - Topic", "");

                queueString += title + ", by " + author;
                if (song == Program.Instance.SongQueues[user.Guild.Id].CurrentSong()) { queueString += " (Current Song)"; }
                queueString += Environment.NewLine;
            }

            TimeSpan totalLength = new TimeSpan(0);

            foreach (var song in Program.Instance.SongQueues[user.Guild.Id].GetAllTracks())
            {
                totalLength += song.Length;
            }

            var embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = "Current queue:",
                Description = queueString,
                Color = Color.Orange
                //ImageUrl = "https://img.youtube.com/vi/" + track.Identifier + "/0.jpg"
            };
            embed.WithCurrentTimestamp();
            embed.WithFooter(totalLength.ToString());
            return embed;
        }
        public static async Task<string[]> GetVideoLinksFromPlaylist(string playlistLink)
        {

            var youtube = new YoutubeExplode.YoutubeClient();
            var playlist = await youtube.Search.GetPlaylistsAsync(playlistLink);

            List<string> urls = new List<string>();



            foreach (var item in await youtube.Playlists.GetVideosAsync(playlist[0].Id))
            {
                urls.Add(item.Url);
            }
            return urls.ToArray();
        }
        public static async Task<string[]> GetPlaylistVideosAsync(string link)
        {
            var youtube = new YoutubeExplode.YoutubeClient();
            // Get playlist metadata
            var playlist = await youtube.Playlists.GetAsync(link);

            var title = playlist.Title; // "First Steps - Blender 2.80 Fundamentals"
            var author = playlist.Author; // "Blender"

            // Get all playlist videos
            var playlistVideos = await youtube.Playlists.GetVideosAsync(playlist.Id);
            string[] links = new string[playlistVideos.Count];

            for (int i = 0; i < playlistVideos.Count; i++)
            {
                links[i] = playlistVideos[i].Url;
            }

            return links;
        }
    }

}