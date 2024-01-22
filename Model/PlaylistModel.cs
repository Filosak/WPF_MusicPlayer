using CommunityToolkit.Mvvm.ComponentModel;
using Music_player_working.Model;
using Music_Player_working.UserControls;
using System.Collections.Generic;

namespace Music_Player_working.Model
{
    public partial class PlaylistModel : ObservableObject // potenciální optimalizace - odstranit songsInPlaylist a vše dělat přes pozice songs
    {
        [ObservableProperty] private string name;
        [ObservableProperty] private string description;
        [ObservableProperty] private bool checkedInPopup;
        public List<int> SongsPositions { get; set; }
        private List<SongModel> songsInPlaylist;

        public PlaylistModel(string name, string description) 
        {
            Name = name;
            Description = description;
            songsInPlaylist = new List<SongModel>();
            SongsPositions = new List<int>();
            checkedInPopup = false;
        }

        public void AddSong(SongModel song)
        {
            if (!songsInPlaylist.Contains(song))
            {
                songsInPlaylist.Add(song);
            }
        }

        public void RemoveSong(SongModel song)
        {
            if (songsInPlaylist.Contains(song))
            {
                songsInPlaylist?.Remove(song);
            }
        }

        public void SetSongPositions()
        {
            SongsPositions.Clear();
            foreach (var item in songsInPlaylist)
            {
                SongsPositions.Add(item.Possition);
            }
        }

        public List<SongModel> GetSongs()
        {
            return songsInPlaylist;
        }

        public void SwapSongsInPlaylist(SongModel songA, SongModel songB)
        {
            int songAIndex = songsInPlaylist.IndexOf(songA);
            int songBIndex = songsInPlaylist.IndexOf(songB);
            songsInPlaylist[songAIndex] = songB;
            songsInPlaylist[songBIndex] = songA;
        }

        public bool Contains(SongModel song)
        {
            return songsInPlaylist.Contains(song);
        }
    }
}
