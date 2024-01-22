using CommunityToolkit.Mvvm.ComponentModel;
using Music_Player_working.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Music_player_working.Model
{
    public partial class SongModel : ObservableObject
    {
        public string Name { get; set; }
        public string Lenght { get; set; }
        public TimeSpan LenghtInTicks { get; set; }
        public string AuthorName { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public Uri SongPath { get; set; }
        public Uri ThumbnailPath { get; set; }


        public bool isInHistory { get; set; }
        [ObservableProperty] private bool isInPlaylistHistory;
        public string? playlistNameInHistory { get; set; }
        public int? possitionInHistory { get; set; }

        public string SongFullPath { 
            get
            {
                return Path.GetFullPath(SongPath.ToString());
            }
        }
        public string ThumbnailFullPath
        {
            get
            {
                return Path.GetFullPath(ThumbnailPath.ToString());
            }
        }

        [ObservableProperty]
        private int possition;

        [ObservableProperty]
        private bool isSelected = false;

        [ObservableProperty] private bool selected;

        public SongModel(string name, TimeSpan lenght, string authorName, string videoUrl, string thumbnailUrl, string songPath, string thumbnailPath, int pos)
        {
            Name = name;
            LenghtInTicks = lenght;
            AuthorName = authorName;
            VideoUrl = videoUrl;
            ThumbnailUrl = thumbnailUrl;
            SongPath = new Uri(songPath, UriKind.Relative);
            ThumbnailPath = new Uri(thumbnailPath, UriKind.Relative);
            possition = pos;

            Lenght = new DateTime(LenghtInTicks.Ticks).ToString("HH:mm:ss");
        }

        public bool Equals(SongModel other) {
            return (other.Name == Name && other.AuthorName == AuthorName);
        }

        public void AddToHistory(int pos, PlaylistModel? playedFromPlaylist = null)
        {
            isInHistory = true;
            possitionInHistory = pos;

            if (playedFromPlaylist != null)
            {
                IsInPlaylistHistory = true;
                playlistNameInHistory = playedFromPlaylist.Name;
            }
        }

        public void RemoveFromHistory()
        {
            isInHistory = false;
            IsInPlaylistHistory = false;
            playlistNameInHistory = null;
            possitionInHistory = null;
        }
    }
}
