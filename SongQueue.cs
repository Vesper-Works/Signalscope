using System;
using System.Collections.Generic;
using System.Text;
using SharpLink;

namespace DiscordMusicBot
{
    class SongQueue
    {
        List<LavalinkTrack> tracks = new List<LavalinkTrack>();
        int pointer = 0;

        public bool IsEmpty { get => tracks.Count == 0; }
        public int Count { get => tracks.Count; }
        public LavalinkTrack Enqueue(LavalinkTrack track)
        {
            tracks.Add(track);
            return track;
        }

        public LavalinkTrack CurrentSong()
        {
            return tracks[pointer];
        }  
        public LavalinkTrack NextSong()
        {
            pointer++;
            CheckForLoop();
            return tracks[pointer];
        }  
        public LavalinkTrack PreviousSong()
        {
            pointer--;
            CheckForLoop();
            return tracks[pointer];
        }
        public void RemoveSong(int position)
        {
            tracks.RemoveAt(position);
        }
        public LavalinkTrack[] GetAllTracks()
        {
            return tracks.ToArray();
        }



        private void CheckForLoop()
        {
            if(pointer == -1) { pointer = tracks.Count - 1; }
        
            if(pointer == tracks.Count) { pointer = 0; }
        }
       
    }
}
